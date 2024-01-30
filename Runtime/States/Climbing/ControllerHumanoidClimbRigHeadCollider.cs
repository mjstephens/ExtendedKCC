using System;
using UnityEngine;

namespace GalaxyGourd.KCC
{
    public class ControllerHumanoidClimbRigHeadCollider : MonoBehaviour
    {
        #region VARIABLES

        [Header("References")]
        [SerializeField] private ControllerHumanoidClimbRig _climbRig;
        
        #endregion VARIABLES
        
        
        #region PHYSICS

        private void OnCollisionStay(Collision collision)
        {
            _climbRig.OnHeadCollisionStay(collision);
        }

        private void OnCollisionExit(Collision collision)
        {
            _climbRig.OnHeadCollisionExit(collision);
        }

        #endregion PHYSICS
    }
}