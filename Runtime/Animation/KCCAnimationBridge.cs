using System;
using UnityEngine;

namespace GalaxyGourd.KCC
{
    public class KCCAnimationBridge : MonoBehaviour, IKCCAnimationProvider
    {
        #region VARIABLES
        
        [Header("References")]
        [SerializeField] private DataConfigKCCAnimation _config;
        
        public DataConfigKCCAnimation Config => _config;
        public float ForwardSpeed { get; private set; }
        public float VerticalSpeed { get; private set; }
        public float LateralSpeed { get; private set; }
        public float TurningSpeed { get; private set; }
        public bool OnRightFoot { get; private set; }
        public bool Ungrounded { get; private set; }
        public bool Jumped { get; private set; }
        public bool Fall { get; private set; }
        public bool Strafe { get; private set; }
        public float SpeedMultiplier { get; private set; }
        
        public Action<KCCGroundedLocomotionType, KCCGroundedLocomotionType> GroundedLocomotionTypeChanged { get; set; }
        public Action<ControllerKCCStateGroundedMoveType, ControllerKCCStateGroundedMoveType> GroundedMoveTypeChanged { get; set; }
        public Action<ControllerKCCStateGroundedPosture, ControllerKCCStateGroundedPosture> GroundedPostureChanged { get; set; }
        public Action DoLand { get; set; }

        // Private calculations
        private Vector3 _animLocalVelocity;
        private float _animStrafe;
        private float _animTurnRotation;
        private float _animTurnDelta;

        #endregion VARIABLES


        #region INITIALIZATION

        private void Awake()
        {
            // Get the animator
            
        }

        #endregion INITIALIZATION


        #region UPDATE

        void IKCCAnimationProvider.UpdateAnimationProperties(float delta, ControllerKCC controller)
        {
            // Set forward speed
            _animLocalVelocity = Vector3.Lerp(
                _animLocalVelocity, 
                controller.LocalVelocity, 
                delta * Config.ForwardSpeedSmoothLerpRate);

            // Set translation speed components
            ForwardSpeed = _animLocalVelocity.z;
            LateralSpeed = _animLocalVelocity.x;
            VerticalSpeed = _animLocalVelocity.y;
            
            // Should we be strafing?
            float strafeSpeed = Mathf.Abs(_animLocalVelocity.x);
            _animStrafe = Mathf.Lerp(_animStrafe, strafeSpeed, delta * 15);
            Strafe = GetShouldStrafe(controller);
            
            // Calculate turn animation value
            float raw = transform.eulerAngles.y - _animTurnRotation;
            switch (raw)
            {
                case > 300: raw -= 360; break;
                case < -300: raw += 360; break;
            }
            _animTurnDelta = Mathf.Lerp(_animTurnDelta, raw, delta * Config.TurningSpeedSmoothLerpRate);
            
            //
            Ungrounded = controller.CurrentState == ControllerKCCStateKey.Ungrounded;
            
            // Update animator and cache value
            TurningSpeed = _animTurnDelta;
            _animTurnRotation = transform.eulerAngles.y;
        }

        private bool GetShouldStrafe(ControllerKCC controller)
        {
            // Must be grounded
            if (controller.CurrentState != ControllerKCCStateKey.Grounded)
                return false;
            
            switch (controller.Operator.ViewState)
            {
                case KCCCameraViewState.FirstPerson:
                    
                    return _animLocalVelocity.z < 0 || (controller.CurrentMovementCollision == KCCMovementCollisionType.Front &&
                                                        _animStrafe > 0.15f);
                    
                    break;
                
                case KCCCameraViewState.ThirdPerson:
                default:

                    return controller.CurrentMovementCollision == KCCMovementCollisionType.Front &&
                           _animStrafe > 0.15f;
                    
                    break;
            }
        }
        
        #endregion UPDATE


        #region EVENTS

        void IKCCAnimationProvider.OnJump()
        {
            Jumped = true;
        }

        void IKCCAnimationProvider.OnFallBegin()
        {
            Fall = true;
        }

        void IKCCAnimationProvider.OnLand()
        {
            Jumped = false;
            Fall = false;
            
            DoLand?.Invoke();
        }

        #endregion EVENTS
    }
}