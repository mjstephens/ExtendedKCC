using UnityEngine;

namespace GalaxyGourd.KCC
{
    public abstract class DataConfigKCCVirtualCamera : ScriptableObject
    {
        [Header("Rotation")]
        public bool InvertX;
        public bool InvertY;
        [Range(-90f, 90f)]
        public float MinVerticalAngle = -90f;
        [Range(-90f, 90f)]
        public float MaxVerticalAngle = 90f;
        public float RotationSpeed = 1f;
        public float RotationSharpness = 50;
    }
}