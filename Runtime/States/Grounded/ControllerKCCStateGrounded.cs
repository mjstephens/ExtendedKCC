using KinematicCharacterController;
using UnityEngine;

namespace GalaxyGourd.KCC
{
    public class ControllerKCCStateGrounded : ControllerKCCState
    {
        #region VARIABLES
        
        internal override ControllerKCCStateKey Key => ControllerKCCStateKey.Grounded;

        private float _jumpStrength => _controller.Config.JumpUpSpeed;// _controller.GameValues.DVJumpStrength;
        private KCCGroundedLocomotionType _currentLocomotion = KCCGroundedLocomotionType.Run;
        private ControllerKCCStateGroundedMoveType _moveType = ControllerKCCStateGroundedMoveType.Default;
        private ControllerKCCStateGroundedPosture _posture = ControllerKCCStateGroundedPosture.Upright;
        private bool _jumpRequested;
        private bool _jumpConsumed;
        private bool _jumpedThisFrame;
        private float _timeSinceJumpRequested = Mathf.Infinity;
        private float _timeSinceLastAbleToJump;
        private bool _sliding => _controller.Motor.GroundingStatus is { IsStableOnGround: false, FoundAnyGround: true };
        
        // Sprint properties
        private bool _sprintRequested;
        
        // Walk properties
        private bool _walkRequested;
        
        // Crouch properties
        private bool _crouchRequested;
        private bool _crouchMultiTapRequested;
        private readonly float _crouchStateCrouchedAbsoluteHeight;
        private readonly float _crouchStateProneAbsoluteHeight;
        private bool _shouldBeCrouching;
        private bool _isCrouching;

        #endregion VARIABLES
        
        
        #region CONSTRUCTION

        public ControllerKCCStateGrounded(ControllerKCC controller) : base(controller)
        {
            _crouchStateCrouchedAbsoluteHeight = 
                _controller.Config.StandingCapsuleHeight * _controller.Config.CrouchedCapsuleHeightMultiplier;
            _crouchStateProneAbsoluteHeight = 
                _controller.Config.StandingCapsuleHeight * _controller.Config.ProneCapsuleHeightMultiplier;
        }

        #endregion CONSTRUCTION
        
        
        #region TRANSITION

        public override void TransitionToState(ControllerKCCStateKey from, string transitionData)
        {
            base.TransitionToState(from, transitionData);
            
            _controller.Motor.SetGroundSolvingActivation(true);
            _controller.AnimatorProvider?.OnLand();
        }
        
        public override void TransitionFromState(ControllerKCCStateKey to)
        {
            base.TransitionFromState(to);
            
        }

        #endregion TRANSITION
        
        
        #region SUBSTATE

        internal override void ForwardInputValues(DataInputValuesControllerKCC input)
        {
            base.ForwardInputValues(input);
            
            if (input.Jump.Started)
            {
                _timeSinceJumpRequested = 0;
                _jumpRequested = true;
            }
            else if (input.Sprint.Started)
            {
                _sprintRequested = true;
            }
            else if (input.Walk.Started)
            {
                _walkRequested = true;
            }
            else if (input.Crouch.Started)
            {
                _crouchRequested = true;
            }
        }

        #endregion SUBSTATE
        
        
        #region UPDATES

        internal override void BeforeCharacterUpdate(float delta)
        {
            base.BeforeCharacterUpdate(delta);
            
            // If we are submerged in water, we need to transition to the swimming state
            // if (_controller.CharacterIsSubmerged())
            // {
            //     _controller.TransitionFromToState(ControllerHumanoidStatus.Grounded, ControllerHumanoidStatus.Swimming);
            // }
        }

        internal override void AfterCharacterUpdate(float delta)
        {
            base.AfterCharacterUpdate(delta);
            
            ProcessJump(delta);
        }

        internal override void ProcessLocomotionState(float delta)
        {
            // If we're sliding, we know the locomotion state
            KCCGroundedLocomotionType currentType;
            if (_sliding)
            {
                currentType = KCCGroundedLocomotionType.SlideUnstable;
            }
            else
            {
                switch (_posture)
                {
                    case ControllerKCCStateGroundedPosture.Crouched: currentType = KCCGroundedLocomotionType.CrouchWalk; break;
                    case ControllerKCCStateGroundedPosture.Prone: currentType = KCCGroundedLocomotionType.ProneCrawl; break;
                    case ControllerKCCStateGroundedPosture.Upright:
                    default:

                        if (_controller.LocalVelocity.z <= 0.1f)
                        {
                            currentType = KCCGroundedLocomotionType.Idle;
                        }
                        else if (_controller.LocalVelocity.z <= _controller.Config.DefaultWalkSpeed)
                        {
                            currentType = KCCGroundedLocomotionType.Walk;
                        }
                        else if (_controller.LocalVelocity.z <= _controller.Config.DefaultRunSpeed)
                        {
                            currentType = KCCGroundedLocomotionType.Run;
                        }
                        else if (_controller.LocalVelocity.z <= _controller.Config.DefaultSprintSpeed + 1)
                        {
                            currentType = KCCGroundedLocomotionType.Sprint;
                        }
                        else
                        {
                            currentType = KCCGroundedLocomotionType.Idle;
                        }
                        
                        break;
                }
            }

            if (currentType != _currentLocomotion)
            {
                _controller.AnimatorProvider?.GroundedLocomotionTypeChanged?.Invoke(_currentLocomotion, currentType);
                _currentLocomotion = currentType;
            }
        }

        #endregion UPDATES


        #region UNGROUND

        internal override void OnCharacterUngrounded()
        {
            _controller.TransitionFromToState(
                ControllerKCCStateKey.Grounded, 
                ControllerKCCStateKey.Ungrounded,
                _jumpedThisFrame? KEY_ToUngroundedFromJump : KEY_ToUngroundedFromFall);
        }

        #endregion UNGROUND


        #region VELOCITY

        internal override void UpdateVelocity(ref Vector3 currentVelocity, Vector3 moveVector, float delta)
        {
            if (_controller.Motor.GroundingStatus.IsStableOnGround)
            {
                UpdateGroundStable(ref currentVelocity, moveVector, delta);
            }
            // Prevent air-climbing sloped walls
            else if (_controller.Motor.GroundingStatus.FoundAnyGround)
            {                
                UpdateGroundUnstable(ref currentVelocity, moveVector, delta);
            }
        }
        
        private float GetMaxStableMoveSpeed()
        {
            float speed = 0;
            switch (_moveType)
            {
                case ControllerKCCStateGroundedMoveType.Walking: speed = _controller.Config.DefaultWalkSpeed; break;
                case ControllerKCCStateGroundedMoveType.Default:  speed = _controller.Config.DefaultRunSpeed; break;
                case ControllerKCCStateGroundedMoveType.Sprinting: speed = _controller.Config.DefaultSprintSpeed; break;
            }

            _controller.MaxStableMoveSpeed = speed;
            return speed;
        }
        
        #endregion VELOCITY
        
        
        #region BEHAVIOR

        /// <summary>
        /// When the controller is on stable ground
        /// </summary>
        private void UpdateGroundStable(ref Vector3 currentVelocity, Vector3 moveVector, float delta)
        {
            float currentVelocityMagnitude = currentVelocity.magnitude;

            Vector3 effectiveGroundNormal = _controller.Motor.GroundingStatus.GroundNormal;
            if (currentVelocityMagnitude > 0f && _controller.Motor.GroundingStatus.SnappingPrevented)
            {
                // Take the normal from where we're coming from
                Vector3 groundPointToCharacter = 
                    _controller.Motor.TransientPosition - _controller.Motor.GroundingStatus.GroundPoint;
                if (Vector3.Dot(currentVelocity, groundPointToCharacter) >= 0f)
                {
                    effectiveGroundNormal = _controller.Motor.GroundingStatus.OuterGroundNormal;
                }
                else
                {
                    effectiveGroundNormal = _controller.Motor.GroundingStatus.InnerGroundNormal;
                }
            }

            // Reorient velocity on slope
            currentVelocity = _controller.Motor.GetDirectionTangentToSurface(
                                  currentVelocity,
                                  effectiveGroundNormal) * currentVelocityMagnitude;

            // Calculate target velocity
            Vector3 inputRight = Vector3.Cross(moveVector, _controller.Motor.CharacterUp);
            Vector3 reorientedInput = Vector3.Cross(effectiveGroundNormal, inputRight).normalized * moveVector.magnitude;
            Vector3 targetMovementVelocity = reorientedInput * GetMaxStableMoveSpeed();
            
            // Smooth movement Velocity
            currentVelocity = Vector3.Lerp(
                currentVelocity, 
                targetMovementVelocity, 
                1f - Mathf.Exp(-_controller.Config.StableMovementSharpness * delta));

            // Check for substates
            TryJump(ref currentVelocity, moveVector, delta);
            TrySprint(delta);
            TryWalk(delta);
            TryCrouch(delta);
        }

        /// <summary>
        /// When the character is on steep/sloped ground
        /// </summary>
        private void UpdateGroundUnstable(ref Vector3 currentVelocity, Vector3 moveVector, float delta)
        {
            Vector3 addedVelocity = moveVector * (_controller.Config.AirAccelerationSpeed * delta);
                
            if (Vector3.Dot(currentVelocity + addedVelocity, addedVelocity) > 0f)
            {
                Vector3 groundNormal = Vector3.Cross(
                    _controller.Motor.CharacterUp, 
                    _controller.Motor.GroundingStatus.GroundNormal);
                Vector3 perpenticularObstructionNormal = Vector3.Cross(
                    groundNormal, 
                    _controller.Motor.CharacterUp).normalized;
                addedVelocity = Vector3.ProjectOnPlane(addedVelocity, perpenticularObstructionNormal);
                    
                currentVelocity += addedVelocity;

                if (_controller.Config.AllowJumpingWhenSliding)
                {
                    TryJump(ref currentVelocity, moveVector, delta);
                }
            }
        }

        #endregion BEHAVIOR
        
        
        #region JUMP

        private void TryJump(ref Vector3 currentVelocity, Vector3 moveVector, float delta)
        {
            _jumpedThisFrame = false;
            _timeSinceJumpRequested += delta;
            if (_jumpRequested)
            {
                // See if we actually are allowed to jump
                if (!_jumpConsumed && _timeSinceLastAbleToJump <= _controller.Config.JumpPostGroundingGraceTime)
                {
                    DoJump(ref currentVelocity, moveVector);
                }
            }
        }

        private void DoJump(ref Vector3 currentVelocity, Vector3 moveVector)
        {
            // Calculate jump direction before ungrounding
            Vector3 jumpDirection = _controller.Motor.CharacterUp;
            if (_controller.Motor.GroundingStatus is { FoundAnyGround: true, IsStableOnGround: false })
            {
                jumpDirection = _controller.Motor.GroundingStatus.GroundNormal;
            }

            // Makes the character skip ground probing/snapping on its next update. 
            // If this line weren't here, the character would remain snapped to the ground when trying to jump
            _controller.Motor.ForceUnground();

            // Add to the return velocity and reset jump state
            currentVelocity += (jumpDirection * _jumpStrength) - 
                               Vector3.Project(currentVelocity, _controller.Motor.CharacterUp);
            currentVelocity += (moveVector * _controller.Config.JumpScalableForwardSpeed);
            _jumpRequested = false;
            _jumpConsumed = true;
            _jumpedThisFrame = true;
            
            _controller.AnimatorProvider?.OnJump();
        }
        
        private void ProcessJump(float deltaTime)
        {
            // Handle jumping pre-ground grace period
            if (_jumpRequested && _timeSinceJumpRequested > _controller.Config.JumpPreGroundingGraceTime)
            {
                _jumpRequested = false;
            }
            else
            {
                _timeSinceJumpRequested += deltaTime;
            }

            if (_controller.Config.AllowJumpingWhenSliding
                    ? _controller.Motor.GroundingStatus.FoundAnyGround
                    : _controller.Motor.GroundingStatus.IsStableOnGround)
            {
                // If we're on a ground surface, reset jumping values
                if (!_jumpedThisFrame)
                {
                    _jumpConsumed = false;
                }

                _timeSinceLastAbleToJump = 0f;
            }
            else
            {
                // Keep track of time since we were last able to jump (for post grace period)
                _timeSinceLastAbleToJump += deltaTime;
            }
        }

        #endregion JUMP


        #region SPRINT

        private void TrySprint(float delta)
        {
            if (_sprintRequested)
            {
                DoSprint(delta);
            }
        }

        private void DoSprint(float delta)
        {
            SetRunState(ControllerKCCStateGroundedMoveType.Sprinting);
            
            _sprintRequested = false;
        }
        
        private void SetRunState(ControllerKCCStateGroundedMoveType state)
        {
            // We can toggle back to the default state (running)
            if (_moveType == state)
            {
                _controller.AnimatorProvider?.GroundedMoveTypeChanged?.Invoke(
                    _moveType, ControllerKCCStateGroundedMoveType.Default);
                _moveType = ControllerKCCStateGroundedMoveType.Default;
            }
            else
            {
                // We can't transition run states if we aren't upright
                if (_posture == ControllerKCCStateGroundedPosture.Upright)
                {
                    _controller.AnimatorProvider?.GroundedMoveTypeChanged?.Invoke(_moveType, state);
                    _moveType = state;
                }
                else
                {
                    switch (_posture)
                    {
                        case ControllerKCCStateGroundedPosture.Crouched:
                            Debug.Log($"State conflict: {state} while crouched.");
                            break;
                        case ControllerKCCStateGroundedPosture.Prone:
                            Debug.Log($"State conflict: {state} while prone.");
                            break;
                    }
                }
            }
        }

        #endregion SPRINT


        #region WALK
        
        private void TryWalk(float delta)
        {
            if (_walkRequested)
            {
                DoWalk(delta);
            }
        }

        /// <summary>
        /// Walking is a shortcut on keyboard input devices to manually limit max movement speed to walk; not needed on gamepad
        /// </summary>
        /// <param name="delta"></param>
        private void DoWalk(float delta)
        {
            SetRunState(ControllerKCCStateGroundedMoveType.Walking);
            
            _walkRequested = false;
        }

        #endregion WALK
        
        
        #region CROUCH

        private void TryCrouch(float delta)
        {
            if (_crouchMultiTapRequested)
            {
                DoCrouch(true);
            }
            else if (_crouchRequested)
            {
                DoCrouch(false);
            }
        }

        private void DoCrouch(bool isMultiTap)
        {
            ToggleCrouchState(isMultiTap);
            
            _crouchRequested = false;
            _crouchMultiTapRequested = false;
        }
        
        private void ToggleCrouchState(bool isMultiTap)
        {
            switch (_posture)
            {
                case ControllerKCCStateGroundedPosture.Upright:
                    TrySetCrouchState(isMultiTap ? 
                        ControllerKCCStateGroundedPosture.Prone : ControllerKCCStateGroundedPosture.Crouched);
                    break;
                case ControllerKCCStateGroundedPosture.Crouched:
                    TrySetCrouchState(isMultiTap ? 
                        ControllerKCCStateGroundedPosture.Prone : ControllerKCCStateGroundedPosture.Upright);
                    break;
                case ControllerKCCStateGroundedPosture.Prone:
                    TrySetCrouchState(isMultiTap ? 
                        ControllerKCCStateGroundedPosture.Upright : ControllerKCCStateGroundedPosture.Crouched);
                    break;
            }
        }

        private void TrySetCrouchState(ControllerKCCStateGroundedPosture state)
        {
            // We can only set crouch state if we aren't sprinting
            if (_moveType == ControllerKCCStateGroundedMoveType.Sprinting)
            {
                switch (state)
                {
                    case ControllerKCCStateGroundedPosture.Crouched:
                        Debug.Log($"State conflict: {state} while sprinting.");
                        break;
                    case ControllerKCCStateGroundedPosture.Prone:
                        Debug.Log($"State conflict: {state} while sprinting.");
                        break;
                }
                
                return;
            }
            
            if (state > _posture)
            {
                // We are crouching down... this should always be allowed
                ResolveNewCrouchState(state);
            }
            else
            {
                // We are trying to get up... we need to do an obstruction check to make sure we have room to stand
                float targetSize = _controller.Config.StandingCapsuleHeight;
                switch (state)
                {
                    case ControllerKCCStateGroundedPosture.Crouched: targetSize = _crouchStateCrouchedAbsoluteHeight; break;
                    case ControllerKCCStateGroundedPosture.Prone: targetSize = _crouchStateProneAbsoluteHeight; break;
                }
                
                if (_controller.TryApplyNewMotorCapsuleDimensions(targetSize, targetSize * 0.5f))
                {
                    ResolveNewCrouchState(state, false);
                }
                else
                {
                    CrouchStateResolutionFailure(_posture, state);
                }
            }
        }

        /// <summary>
        /// Called when we have verified that the target crouch state is valid and able to be switched to
        /// </summary>
        private void ResolveNewCrouchState(ControllerKCCStateGroundedPosture state, bool doSetCapsuleHeight = true)
        {
            ControllerKCCStateGroundedPosture previousState = _posture;

            switch (state)
            {
                case ControllerKCCStateGroundedPosture.Upright: 
                    ApplyCrouchStateUpright(previousState, doSetCapsuleHeight); 
                    break;
                case ControllerKCCStateGroundedPosture.Crouched: 
                    ApplyCrouchStateCrouched(previousState, doSetCapsuleHeight); 
                    break;
                case ControllerKCCStateGroundedPosture.Prone: 
                    ApplyCrouchStateProne(previousState, doSetCapsuleHeight); 
                    break;
            }
            
           _controller.AnimatorProvider?.GroundedPostureChanged?.Invoke(previousState, state);
           _posture = state;
        }

        private void ApplyCrouchStateUpright(ControllerKCCStateGroundedPosture previousState, bool doSetCapsuleHeight = true)
        {
            if (doSetCapsuleHeight)
            {
                _controller.Motor.SetCapsuleDimensions(
                    0.5f,
                    _controller.Config.StandingCapsuleHeight,
                    _controller.Config.StandingCapsuleHeight * 0.5f);
            }
            
            _controller.MeshRoot.localScale = new Vector3(1f, 1f, 1f);
        }
        
        private void ApplyCrouchStateCrouched(ControllerKCCStateGroundedPosture previousState, bool doSetCapsuleHeight = true)
        {
            if (doSetCapsuleHeight)
            {
                _controller.Motor.SetCapsuleDimensions(
                    0.5f,
                    _crouchStateCrouchedAbsoluteHeight,
                    _crouchStateCrouchedAbsoluteHeight * 0.5f);
            }
            
            _controller.MeshRoot.localScale = new Vector3(1f, _controller.Config.CrouchedCapsuleHeightMultiplier, 1f);
        }
        
        private void ApplyCrouchStateProne(ControllerKCCStateGroundedPosture previousState, bool doSetCapsuleHeight = true)
        {
            if (doSetCapsuleHeight)
            {
                _controller.Motor.SetCapsuleDimensions(
                    0.5f,
                    _crouchStateProneAbsoluteHeight,
                    _crouchStateProneAbsoluteHeight * 0.5f);
            }
            
            _controller.MeshRoot.localScale = new Vector3(1f, _controller.Config.ProneCapsuleHeightMultiplier, 1f);
        }

        private void CrouchStateResolutionFailure(
            ControllerKCCStateGroundedPosture fromState, 
            ControllerKCCStateGroundedPosture toState)
        {
            Debug.Log($"Failed to resolve from {fromState} to {toState}");
        }

        #endregion CROUCH


        #region COLLISION

        internal override void OnMovementHit(
            Collider hitCollider, 
            Vector3 hitNormal, 
            Vector3 hitPoint, 
            ref HitStabilityReport hitStabilityReport)
        {
            base.OnMovementHit(hitCollider, hitNormal, hitPoint, ref hitStabilityReport);
            
            Vector3 heading = new Vector3(hitPoint.x, _controller.transform.position.y, hitPoint.z) - 
                              _controller.transform.position;
            float dot = Vector3.Dot(heading, _controller.transform.forward);

            // Set collision type based on hit direction
            _controller.CurrentMovementCollision = dot > 0 ? KCCMovementCollisionType.Front : KCCMovementCollisionType.Back;
        }

        #endregion COLLISION
    }
}