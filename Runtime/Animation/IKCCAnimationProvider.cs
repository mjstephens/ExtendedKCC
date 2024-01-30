using System;

namespace GalaxyGourd.KCC
{
    public interface IKCCAnimationProvider
    {
        #region PROPERTIES

        DataConfigKCCAnimation Config { get; }
        
        float ForwardSpeed { get; }
        float VerticalSpeed { get; }
        float LateralSpeed { get; }
        float TurningSpeed { get; }

        bool OnRightFoot { get; }
        bool Ungrounded { get; }
        bool Jumped { get; }
        bool Fall { get; }
        bool Strafe { get; }
        float SpeedMultiplier { get; }

        Action<KCCGroundedLocomotionType, KCCGroundedLocomotionType> GroundedLocomotionTypeChanged { get; set; }
        Action<ControllerKCCStateGroundedMoveType, ControllerKCCStateGroundedMoveType> GroundedMoveTypeChanged { get; set; }
        Action<ControllerKCCStateGroundedPosture, ControllerKCCStateGroundedPosture> GroundedPostureChanged { get; set; }
        Action DoLand { get; set; }

        #endregion PROPERTIES


        #region METHODS

        /// <summary>
        /// Called after the character update; animates properties based on current KCC state
        /// </summary>
        void UpdateAnimationProperties(float delta, ControllerKCC controller);

        void OnJump();
        void OnFallBegin();
        void OnLand();

        #endregion METHODS
    }
}