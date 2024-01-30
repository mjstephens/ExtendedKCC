using UnityEngine;

namespace GalaxyGourd.KCC
{
    [CreateAssetMenu(
        fileName = "DAT_KCC_TPCam", 
        menuName = "GG/Characters/KCC Third Person Camera")]
    public class DataConfigKCCVirtualCameraTP : DataConfigKCCVirtualCamera
    {
        [Header("Third Person")]
        public Vector2 FollowDistanceRange;
        public float ZoomAdjustMultiplier = 0.25f;
        public float ZoomAdjustDecayRate = 0.25f;
    }
}