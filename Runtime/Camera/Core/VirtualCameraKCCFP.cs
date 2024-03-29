using UnityEngine;

namespace GalaxyGourd.KCC
{
    /// <summary>
    /// First-person camera for controlling humanoids
    /// </summary>
    public class VirtualCameraKCCFP : VirtualCameraKCC
    {
        #region INITIALIZATION

        public override void Init(IKCCOperator op, ControllerKCC kcc)
        {
            base.Init(op, kcc);
            
           VCam.Follow = kcc.CameraTargetFP;
        }

        #endregion INITIALIZATION


        #region TICK

        protected override void PostTick(float delta)
        {
            base.PostTick(delta);

            if (_operator.ViewState == KCCCameraViewState.FirstPerson && _kcc)
            {
                _kcc.Motor.SetRotation(Quaternion.Euler(
                    _controllerTransform.eulerAngles.x, 
                    transform.eulerAngles.y, 
                    _controllerTransform.eulerAngles.z));
                _kcc.MeshRoot.transform.localEulerAngles = new Vector3(
                    _kcc.MeshRoot.transform.localEulerAngles.x,
                    0,
                    _kcc.MeshRoot.transform.localEulerAngles.z);
            }
        }

        #endregion TICK
    }
}