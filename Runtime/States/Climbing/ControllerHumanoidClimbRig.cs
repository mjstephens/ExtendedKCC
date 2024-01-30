using GalaxyGourd.Utility.Core;
using UnityEngine;

namespace GalaxyGourd.KCC
{
    /// <summary>
    /// Assists with freeclimbing novigation and detection
    /// </summary>
    public class ControllerHumanoidClimbRig : MonoBehaviour
    {
        #region VARIABLES

        [Header("References")]
        [SerializeField] private Transform _raySourceTopLeft;
        [SerializeField] private Transform _raySourceTopRight;
        [SerializeField] private Transform _raySourceBottomLeft;
        [SerializeField] private Transform _raySourceBottomRight;
        [SerializeField] private Transform _bottomOverlapSphere;

        public Vector3 TargetPosition => _targetPosition;
        public Quaternion TargetRotation => _targetRotation;

        private Vector3 _centerRayPoint;
        private Vector3 _climbPointOffset;
        private Vector3 _currentClimbNormal;
        private Vector3 _currentClimbPoint;
        private Vector3 _targetPosition;
        private Quaternion _targetRotation;
        private DataConfigKCC _config;
        private ControllerKCC _kcc;
        private ControllerHumanoidStateClimbing _climbState;

        private RaycastHit _topRightHit;
        private RaycastHit _topLeftHit;
        private RaycastHit _bottomRightHit;
        private RaycastHit _bottomLeftHit;
        private bool _enabled = false;
        
        #endregion VARIABLES


        #region INITIALIZATION

        public void Init(
            ControllerHumanoidStateClimbing climbState,
            Transform controller, 
            ControllerKCC kcc, 
            DataConfigKCC config)
        {
            // Set position and rotation of controller to start
            _targetPosition = controller.position;
            _targetRotation = controller.rotation;
            transform.SetPositionAndRotation(_targetPosition, _targetRotation);
            _config = config;
            _kcc = kcc;
            _climbState = climbState;

            _centerRayPoint = (_raySourceTopLeft.position + _raySourceTopRight.position 
                                                          + _raySourceBottomLeft.position + _raySourceBottomRight.position) / 4;
            _climbPointOffset = transform.position - _centerRayPoint;
            _enabled = true;
            
            GatherClimbHits();
        }

        #endregion INITIALIZATION


        #region CLIMBING

        private void FixedUpdate()
        {
            ClimbTick(Time.fixedDeltaTime);
        }

        public void ClimbTick(float delta)
        {
            if (!_enabled)
                return;
            
            GatherClimbHits();
            Quaternion targ = Quaternion.LookRotation(-_currentClimbNormal, Vector3.up);
            _targetRotation = Quaternion.Slerp (transform.rotation, targ, Time.deltaTime * _config.SurfaceOrientationRate);
            _targetPosition = (_currentClimbPoint - (transform.forward * _config.SurfaceOffset)) + _climbPointOffset;
            
            // Recalc offset
            _centerRayPoint = (_raySourceTopLeft.position + _raySourceTopRight.position
                                                          + _raySourceBottomLeft.position + _raySourceBottomRight.position) / 4;
            _climbPointOffset = transform.position - _centerRayPoint;
        }

        private void GatherClimbHits()
        {
            Physics.Raycast(
                _raySourceTopLeft.position, 
                _raySourceTopLeft.forward, 
                out _topLeftHit,
                5, 
                _config.ClimbingLayerMask);
            Physics.Raycast(
                _raySourceTopRight.position,
                _raySourceTopRight.forward,
                out _topRightHit,
                5, 
                _config.ClimbingLayerMask);
            Physics.Raycast(
                _raySourceBottomLeft.position, 
                _raySourceBottomLeft.forward, 
                out _bottomLeftHit,
                5, 
                _config.ClimbingLayerMask);
            Physics.Raycast(
                _raySourceBottomRight.position,
                _raySourceBottomRight.forward,
                out _bottomRightHit,
                5, 
                _config.ClimbingLayerMask);

            if (Physics.Raycast(
                    _centerRayPoint,
                    -Vector3.up,
                out RaycastHit overlapHitBottom,
                5,
                    _config.ClimbingLayerMask))
            {
                _bottomOverlapSphere.position = overlapHitBottom.point;
                if (transform.InverseTransformPoint(overlapHitBottom.point).y >
                    transform.InverseTransformPoint(_raySourceBottomRight.position).y)
                {
                    float groundNormal = MathUtility.Map(overlapHitBottom.normal.y, 0, 1, 90, 0);
                    if (groundNormal <= _kcc.Motor.MaxStableSlopeAngle)
                    {
                        _climbState.HasRechedStableGroundFromClimbRig(overlapHitBottom.point, new Quaternion(0,0,0,0));
                    }
                }
                //Debug.Log(MathUtility.Map(overlapHitBottom.normal.y, 0, 1, 90, 0));
            }
            
            Debug.DrawLine(_raySourceTopLeft.position, _topLeftHit.point, Color.green);
            Debug.DrawLine(_raySourceTopRight.position, _topRightHit.point, Color.green);
            Debug.DrawLine(_raySourceBottomLeft.position, _bottomLeftHit.point, Color.green);
            Debug.DrawLine(_raySourceBottomRight.position, _bottomRightHit.point, Color.green);
            
            _currentClimbNormal = (_topLeftHit.normal + _topRightHit.normal + _bottomLeftHit.normal + _bottomRightHit.normal) / 4;
            _currentClimbPoint = (_topLeftHit.point + _topRightHit.point + _bottomLeftHit.point + _bottomRightHit.point) / 4;
            
            Debug.DrawLine(_currentClimbPoint, _currentClimbPoint + _currentClimbNormal * 5, Color.yellow);
        }

        public void ReceiveClimbInput(Vector3 move)
        {
            Vector3 verticalMove = transform.up * (move.z * _config.ClimbSpeedVertical);
            Vector3 horizontalMove = transform.right * (move.x * _config.ClimbSpeedHorizontal);
            _targetPosition += (verticalMove + horizontalMove);
        }

        public void SyncRigPosition(Transform target)
        {
            // Set position to source
            transform.SetPositionAndRotation(target.position, target.rotation);
        }

        #endregion CLIMBING


        #region HEAD COLLIDER

        internal void OnHeadCollisionStay(Collision collision)
        {
            
        }
        
        internal void OnHeadCollisionExit(Collision collision)
        {
            
        }

        #endregion HEAD COLLIDER
    } 
}