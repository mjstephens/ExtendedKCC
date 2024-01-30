using Unity.Cinemachine;
using UnityEngine;

namespace GalaxyGourd.KCC
{
    public class VirtualCameraKCCTP : VirtualCameraKCC
    {
        #region VARIABLES

        private CinemachinePositionComposer _framingTransposer;
        private float _currentScrollSpeed;

        #endregion VARIABLES


        #region INITIALIZATION

        protected void Awake()
        {
            _framingTransposer = _vCam.GetComponent<CinemachinePositionComposer>();
        }

        public override void Init(IKCCOperator op, ControllerKCC kcc)
        {
            base.Init(op, kcc);
            
            _vCam.Follow = kcc.CameraTargetTP;
        }

        #endregion INITIALIZATION


        #region FUNCTION

        protected override void ProcessCameraZoomInput(float input)
        {
            base.ProcessCameraZoomInput(input);
            
            // Zoom in/out from target in cinematic camera
            if (_operator.ViewState == KCCCameraViewState.ThirdPerson && _config is DataConfigKCCVirtualCameraTP config)
            {
                _currentScrollSpeed = input == 0 ? 0 : input * config.ZoomAdjustMultiplier;
                float targDistance = _framingTransposer.CameraDistance + _currentScrollSpeed;
                
                _framingTransposer.CameraDistance = Mathf.Clamp(
                    targDistance, 
                    config.FollowDistanceRange.x, 
                    config.FollowDistanceRange.y);

                _currentScrollSpeed = Mathf.Lerp(_currentScrollSpeed, 0, Time.deltaTime * config.ZoomAdjustDecayRate);
            }
        }

        internal float IncrementCameraFollowDistance(float increment)
        {
            return 0;//_vCam.GetCinemachineComponent<>()
        }

        #endregion FUNCTION
    }
}