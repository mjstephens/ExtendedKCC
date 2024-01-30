using Unity.Cinemachine;
using UnityEngine;

namespace GalaxyGourd.KCC
{
    /// <summary>
    /// Base class for all Cinemachine brain cameras.
    /// </summary>
    [RequireComponent(typeof(CinemachineBrain))]
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class GameCamera : MonoBehaviour
    {
        #region VARIABLES

        [Header("References")]
        [SerializeField] private UnityEngine.Camera _camera;
        [SerializeField] protected CinemachineBrain _brain;

        public UnityEngine.Camera Camera => _camera;
        public CinemachineBrain Brain => _brain;

        #endregion VARIABLES


        #region UTILITY

        internal void SetCameraRect(Rect rect)
        {
            Camera.rect = rect;
        }

        #endregion UTILITY
    }
}