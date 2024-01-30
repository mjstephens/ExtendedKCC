using KinematicCharacterController;
using UnityEngine;

namespace GalaxyGourd.KCC
{
    public interface IKCCCollisionReceivable
    {
        void ReceiveKCCCollision(Collider hitCollider,
                                 Vector3 hitNormal, 
                                 Vector3 hitPoint, 
                                 Vector3 atCharacterPosition,
                                 Quaternion atCharacterRotation, 
                                 HitStabilityReport hitStabilityReport);
    }
}