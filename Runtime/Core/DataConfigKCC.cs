using UnityEngine;

namespace GalaxyGourd.KCC
{
    [CreateAssetMenu(
        fileName = "DAT_KCC_Config", 
        menuName = "GG/Characters/KCC Config")]
    public class DataConfigKCC : ScriptableObject
    {
        [Header("Stable Movement")]
        public float DefaultWalkSpeed = 2.5f;
        public float DefaultRunSpeed = 6f;
        public float DefaultSprintSpeed = 10f;
        public float StableMovementSharpness = 15f;
        public float OrientationSharpness = 10f;
        public OrientationMethod OrientationMethod = OrientationMethod.TowardsCamera;

        [Header("Air Movement")]
        public float MaxAirMoveSpeed = 15f;
        public float AirAccelerationSpeed = 15f;
        public float Drag = 0.1f;

        [Header("Jumping")]
        public bool AllowJumpingWhenSliding = false;
        public float JumpUpSpeed = 10f;
        public float JumpScalableForwardSpeed = 10f;
        public float JumpPreGroundingGraceTime = 0f;
        public float JumpPostGroundingGraceTime = 0f;

        [Header("Crouching")] 
        public float StandingCapsuleHeight = 1;
        public float CrouchedCapsuleHeightMultiplier = 0.5f;
        public float ProneCapsuleHeightMultiplier = 0.25f;

        [Header("Climbing")] 
        public GameObject PrefabClimbRig;
        public LayerMask ClimbingLayerMask;
        public float SurfaceOffset = 0.5f;
        public float SurfaceOrientationRate = 20f;
        public float ClimbSpeedVertical = 5;
        public float ClimbSpeedHorizontal = 2;

        [Header("Misc")]
        public BonusOrientationMethod InitialBonusOrientationMethod = BonusOrientationMethod.None;
        public float BonusOrientationSharpness = 10f;
        public Vector3 Gravity = new Vector3(0, -30f, 0);
    }
    
    public enum OrientationMethod
    {
        TowardsCamera,
        TowardsMovement,
    }
    
    public enum BonusOrientationMethod
    {
        None,
        TowardsGravity,
        TowardsGroundSlopeAndGravity,
    }
}