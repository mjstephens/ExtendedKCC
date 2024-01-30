using UnityEngine;

namespace GalaxyGourd.KCC
{
    public class ControllerHumanoidStateUngrounded : ControllerKCCState
    {
        #region VARIABLES

        internal override ControllerKCCStateKey Key => ControllerKCCStateKey.Ungrounded;
        private bool _isFalling;
        private bool _stateEnteredFromJump;

        #endregion VARIABLES


        #region CONSTRUCTION

        public ControllerHumanoidStateUngrounded(ControllerKCC controller) : base(controller)
        {
            _isFalling = _controller.Motor.Velocity.y < 0;
        }

        #endregion CONSTRUCTION
        
        
        #region TRANSITION

        public override void TransitionToState(ControllerKCCStateKey from, string transitionData)
        {
            base.TransitionToState(from, transitionData);

            switch (transitionData)
            {
                case KEY_ToUngroundedFromJump:
                    ToStateFromJump();
                    break;
                case KEY_ToUngroundedFromFall:
                    ToStateFromFall();
                    break;
            }
            
            _controller.Motor.SetGroundSolvingActivation(true);
        }
        
        private void ToStateFromJump()
        {
            _stateEnteredFromJump = true;
            _controller.AnimatorProvider?.OnJump();
        }

        private void ToStateFromFall()
        {
            _stateEnteredFromJump = false;
            _controller.AnimatorProvider?.OnFallBegin();
        }

        #endregion TRANSITION
        
        
        #region UPDATES

        internal override void BeforeCharacterUpdate(float delta)
        {
            base.BeforeCharacterUpdate(delta);

            // If we are submerged in water, we need to transition to the swimming state
            // if (_controller.CharacterIsSubmerged())
            // {
            //     _controller.TransitionFromToState(ControllerHumanoidStatus.Ungrounded, ControllerHumanoidStatus.Swimming);
            // }
        }

        #endregion UPDATES
        
        
        #region GROUND

        internal override void OnCharacterGrounded()
        {
            _controller.TransitionFromToState(ControllerKCCStateKey.Ungrounded, ControllerKCCStateKey.Grounded);
        }

        #endregion GROUND
        
        
        #region VELOCITY

        internal override void UpdateVelocity(ref Vector3 currentVelocity, Vector3 moveVector, float delta)
        {
            // Add move input
            if (moveVector.sqrMagnitude > 0f)
            {
                Vector3 addedVelocity = moveVector * (_controller.Config.AirAccelerationSpeed * delta);

                Vector3 currentVelocityOnInputsPlane =
                    Vector3.ProjectOnPlane(currentVelocity, _controller.Motor.CharacterUp);

                // Limit air velocity from inputs
                if (currentVelocityOnInputsPlane.magnitude < _controller.MaxStableMoveSpeed)
                {
                    // clamp addedVel to make total vel not exceed max vel on inputs plane
                    Vector3 newTotal = 
                        Vector3.ClampMagnitude(currentVelocityOnInputsPlane + addedVelocity, _controller.MaxStableMoveSpeed);
                    addedVelocity = newTotal - currentVelocityOnInputsPlane;
                }
                else
                {
                    // Make sure added vel doesn't go in the direction of the already-exceeding velocity
                    if (Vector3.Dot(currentVelocityOnInputsPlane, addedVelocity) > 0f)
                    {
                        addedVelocity = Vector3.ProjectOnPlane(addedVelocity,
                            currentVelocityOnInputsPlane.normalized);
                    }
                }

                // Prevent air-climbing sloped walls
                if (_controller.Motor.GroundingStatus.FoundAnyGround)
                {
                    if (Vector3.Dot(currentVelocity + addedVelocity, addedVelocity) > 0f)
                    {
                        Vector3 perpenticularObstructionNormal =
                            Vector3.Cross(
                                Vector3.Cross(_controller.Motor.CharacterUp, _controller.Motor.GroundingStatus.GroundNormal),
                                _controller.Motor.CharacterUp).normalized;
                        addedVelocity = Vector3.ProjectOnPlane(addedVelocity, perpenticularObstructionNormal);
                    }
                }

                // Apply added velocity
                currentVelocity += addedVelocity;
            }
            
            ApplyGravity(ref currentVelocity, delta);
            ApplyDrag(ref currentVelocity, delta);
            
            if (!_stateEnteredFromJump)
                return;

            // Set current falling status
            bool falling = currentVelocity.y < 0;
            if (falling && !_isFalling)
            {
                _isFalling = true;
                _controller.AnimatorProvider?.OnFallBegin();
            }
            else if (!falling && _isFalling)
            {
                _isFalling = false;
            }
        }

        #endregion VELOCITY
    }
}