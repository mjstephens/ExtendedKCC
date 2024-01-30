using KinematicCharacterController;
using UnityEngine;

namespace GalaxyGourd.KCC
{
    public abstract class ControllerKCCState
    {
        #region VARIABLES

        internal abstract ControllerKCCStateKey Key { get; }
        
        protected readonly ControllerKCC _controller;
        protected const string KEY_ToUngroundedFromJump = "UNG_JUMP";
        protected const string KEY_ToUngroundedFromFall = "UNG_FALL";

        #endregion VARIABLES
        
        
        #region CONSTRUCTION

        internal ControllerKCCState(ControllerKCC controller)
        {
            _controller = controller;
        }

        #endregion CONSTRUCTION
        
        
        #region TRANSITION

        public virtual void TransitionFromState(ControllerKCCStateKey to)
        {
            
        }

        public virtual void TransitionToState(ControllerKCCStateKey from, string transitionData)
        {
            
        }

        #endregion TRANSITION
        
        
        #region SUBSTATE

        internal virtual void ForwardInputValues(DataInputValuesControllerKCC input)
        {
            
        }
        
        #endregion SUBSTATE
        
        
        #region MOTOR UPDATES

        internal virtual void BeforeCharacterUpdate(float deltaTime)
        {
            
        }
        
        internal virtual void OnCharacterGrounded()
        {
            
        }

        internal virtual void OnCharacterUngrounded()
        {
            
        }
        
        // Remember this is only called here when in 3rd person; 1st person rotation is handled by VirtualCamreraKCCFP
        internal virtual void UpdateRotation(ref Quaternion currentRotation, Vector3 lookVector, float delta)
        {
            if (lookVector.sqrMagnitude > 0f && _controller.Config.OrientationSharpness > 0f)
            {
                // Smoothly interpolate from current to target look direction
                Vector3 smoothedLookInputDirection = Vector3.Slerp(
                    _controller.Motor.CharacterForward, 
                    lookVector,
                    1 - Mathf.Exp(-_controller.Config.OrientationSharpness * delta)).normalized;

                // Set the current rotation (which will be used by the KinematicControllerMotor)
                currentRotation = Quaternion.LookRotation(
                    smoothedLookInputDirection, 
                    _controller.Motor.CharacterUp);
            }

            Vector3 currentUp = (currentRotation * Vector3.up);
            switch (_controller.BonusOrientationMethod)
            {
                case BonusOrientationMethod.TowardsGravity:
                {
                    // Rotate from current up to invert gravity
                    Vector3 smoothedGravityDir = Vector3.Slerp(
                        currentUp, 
                        -_controller.Config.Gravity.normalized,
                        1 - Mathf.Exp(-_controller.Config.BonusOrientationSharpness * delta));
                    currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) * currentRotation;
                    break;
                }
                case BonusOrientationMethod.TowardsGroundSlopeAndGravity when _controller.Motor.GroundingStatus.IsStableOnGround:
                {
                    Vector3 initialCharacterBottomHemiCenter =
                        _controller.Motor.TransientPosition + (currentUp * _controller.Motor.Capsule.radius);

                    Vector3 smoothedGroundNormal = Vector3.Slerp(
                        _controller.Motor.CharacterUp,
                        _controller.Motor.GroundingStatus.GroundNormal,
                        1 - Mathf.Exp(-_controller.Config.BonusOrientationSharpness * delta));
                    currentRotation = Quaternion.FromToRotation(currentUp, smoothedGroundNormal) * currentRotation;

                    // Move the position to create a rotation around the bottom hemi center instead of around the pivot
                    _controller.Motor.SetTransientPosition(
                        initialCharacterBottomHemiCenter + 
                        (currentRotation * Vector3.down * _controller.Motor.Capsule.radius));
                    break;
                }
                case BonusOrientationMethod.TowardsGroundSlopeAndGravity:
                {
                    Vector3 smoothedGravityDir = Vector3.Slerp(
                        currentUp, 
                        -_controller.Config.Gravity.normalized,
                        1 - Mathf.Exp(-_controller.Config.BonusOrientationSharpness * delta));
                    currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) * currentRotation;
                    break;
                }
                default:
                {
                    Vector3 smoothedGravityDir = Vector3.Slerp(
                        currentUp, 
                        Vector3.up,
                        1 - Mathf.Exp(-_controller.Config.BonusOrientationSharpness * delta));
                    //currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) * currentRotation;
                    break;
                }
            }
        }

        internal virtual void UpdateVelocity(ref Vector3 currentVelocity, Vector3 moveVector, float delta)
        {
            
        }

        internal virtual void AfterCharacterUpdate(float delta)
        {
            ProcessLocomotionState(delta);
        }
        
        #endregion MOTOR UPDATES


        #region MOTOR EVENTS

        internal virtual void OnDiscreteCollisionDetected(Collider hitCollider)
        {
            
        }
        
        internal virtual void OnGroundHit(
            Collider hitCollider,
            Vector3 hitNormal,
            Vector3 hitPoint,
            ref HitStabilityReport hitStabilityReport)
        {
            
        }

        internal virtual void OnMovementHit(
            Collider hitCollider, 
            Vector3 hitNormal, 
            Vector3 hitPoint, 
            ref HitStabilityReport hitStabilityReport)
        {
            
        }
        
        internal virtual void ProcessHitStabilityReport(
            Collider hitCollider,
            Vector3 hitNormal, 
            Vector3 hitPoint, 
            Vector3 atCharacterPosition,
            Quaternion atCharacterRotation, 
            ref HitStabilityReport hitStabilityReport)
        {
            
        }
        
        #endregion MOTOR EVENTS
        
        
        #region UTILITY

        protected void ApplyGravity(ref Vector3 currentVelocity, float delta)
        {
            currentVelocity += _controller.Config.Gravity * delta;
        }
        
        protected void ApplyDrag(ref Vector3 currentVelocity, float delta)
        {
            currentVelocity *= (1f / (1f + (_controller.Config.Drag * delta)));
        }

        /// <summary>
        /// Called when the associated controller is destroyed
        /// </summary>
        internal virtual void Cleanup()
        {
            
        }

        #endregion UTILITY

        internal virtual void PostGroundingUpdate(float deltaTime)
        {
            
        }

        

        internal virtual bool IsColliderValidForCollisions(Collider coll)
        {
            return false;
        }

        internal virtual void ProcessLocomotionState(float delta)
        {
            
        }
    }
}