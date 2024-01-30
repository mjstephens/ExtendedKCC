using KinematicCharacterController;
using UnityEngine;

namespace GalaxyGourd.KCC
{
    public class ControllerHumanoidStateClimbing : ControllerKCCState
    {
        #region VARIABLES

        internal override ControllerKCCStateKey Key => ControllerKCCStateKey.Climbing;

        private readonly ControllerHumanoidClimbRig _climbRig;

        #endregion VARIABLES


        #region CONSTRUCTION

        public ControllerHumanoidStateClimbing(ControllerKCC controller) : base(controller)
        {
            // _climbRig = 
            //     Object.Instantiate(_controller.Config.PrefabClimbRig).GetComponent<ControllerHumanoidClimbRig>();
        }

        #endregion CONSTRUCTION
        
        
        #region TRANSITION

        public override void TransitionToState(ControllerKCCStateKey from, string transitionData)
        {
            base.TransitionToState(from, transitionData);
            
            //
            _climbRig.gameObject.SetActive(true);
            _climbRig.Init(this, _controller.Motor.transform, _controller, _controller.Config);
        }
        
        public override void TransitionFromState(ControllerKCCStateKey to)
        {
            base.TransitionFromState(to);
            
            _climbRig.gameObject.SetActive(false);
        }

        #endregion TRANSITION


        #region SURFACE CALC

        internal override void BeforeCharacterUpdate(float deltaTime)
        {
            base.BeforeCharacterUpdate(deltaTime);

            // Allow the climb rig to calculate climbing behavior
            _climbRig.ClimbTick(deltaTime);
        }

        internal override void AfterCharacterUpdate(float deltaTime)
        {
            base.AfterCharacterUpdate(deltaTime);
            
            // We might have moved across the climb surface; we need to have the climbing rig match our new position for the next frame
            _climbRig.SyncRigPosition(_controller.Motor.Transform);
        }

        #endregion SURFACE CALC
        
        
        #region ROTATION

        internal override void UpdateRotation(ref Quaternion currentRotation, Vector3 lookVector, float delta)
        {
            _controller.Motor.SetRotation(_climbRig.TargetRotation);
        }

        #endregion ROTATION


        #region VELOCITY

        internal override void UpdateVelocity(ref Vector3 currentVelocity, Vector3 moveVector, float delta)
        {
            currentVelocity = Vector3.zero;
            _climbRig.ReceiveClimbInput(moveVector);
            _controller.Motor.SetPosition(_climbRig.TargetPosition);
        }

        #endregion VELOCITY


        #region GROUNDING

        internal override void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
            base.OnGroundHit(hitCollider, hitNormal, hitPoint, ref hitStabilityReport);
            
            _controller.TransitionFromToState(ControllerKCCStateKey.Climbing, ControllerKCCStateKey.Grounded);
        }

        internal void HasRechedStableGroundFromClimbRig(Vector3 position, Quaternion rotation)
        {
            _controller.Motor.SetPositionAndRotation(position, rotation);
            _controller.TransitionFromToState(ControllerKCCStateKey.Climbing, ControllerKCCStateKey.Grounded);
        }

        #endregion GROUNDING


        #region UTILITY

        internal override void Cleanup()
        {
            base.Cleanup();

            if (_climbRig)
            {
                Object.Destroy(_climbRig.gameObject);
            }
        }

        #endregion UTILITY
    }
}