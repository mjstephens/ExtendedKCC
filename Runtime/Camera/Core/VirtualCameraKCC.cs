using System;
using KinematicCharacterController;
using Unity.Cinemachine;
using UnityEngine;

namespace GalaxyGourd.KCC
{
    [RequireComponent(typeof(CinemachineCamera))]
    public abstract class VirtualCameraKCC : MonoBehaviour
    {
        #region VARIABLES

        [Header("Config")]
        [SerializeField] protected DataConfigKCCVirtualCamera _config;
        
        public CinemachineCamera VCam { get; private set; }

        protected IKCCOperator _operator;
        protected ControllerKCC _kcc;
        protected float _targetVerticalAngle;
        protected Vector3 _planarDirection;
        protected Transform _controllerTransform;

        #endregion VARIABLES


        #region INITIALIZATION

        protected virtual void OnEnable()
        {
            Cursor.lockState = CursorLockMode.Locked;
            _targetVerticalAngle = 0f;
            _planarDirection = Vector3.forward;
        }
        
        public virtual void Init(IKCCOperator op, ControllerKCC kcc)
        {
            VCam = GetComponent<CinemachineCamera>();

            _operator = op;
            _kcc = kcc;
            _controllerTransform = kcc.Motor.Transform;
        }

        #endregion INITIALIZATION
        
        
        #region TICK
        
        public void Tick(float delta)
        {
            if (!_controllerTransform)
                return;
            
            // Get data from operator
            DataInputValuesKCCCamera data  = _operator.CameraInputData;

            // Process camera zoom/view state
            ProcessCameraZoomInput(data.ZoomInput);
            
            // Handle rotating the camera along with physics movers; this should only happen when we are in first-person
            if (_operator.ViewState == KCCCameraViewState.FirstPerson)
            {
                if (_kcc.Motor.AttachedRigidbody && _kcc.Motor.AttachedRigidbody.TryGetComponent(out PhysicsMover pm))
                {
                    _planarDirection = pm.RotationDeltaFromInterpolation * _planarDirection;
                    _planarDirection = Vector3.ProjectOnPlane(
                        _planarDirection, 
                        _kcc.Motor.CharacterUp).normalized;
                }
            }
            
            if (_config.InvertX)
            {
                data.RotationInput.x *= -1f;
            }

            if (_config.InvertY)
            {
                data.RotationInput.y *= -1f;
            }

            // Process rotation input
            Vector3 up = _controllerTransform.up;
            Quaternion rotationFromInput = Quaternion.Euler(up * (data.RotationInput.x * _config.RotationSpeed));
            _planarDirection = rotationFromInput * _planarDirection;
            _planarDirection = Vector3.Cross(up, Vector3.Cross(_planarDirection, up));
            Quaternion planarRot = Quaternion.LookRotation(_planarDirection, up);

            // Apply rotation
            _targetVerticalAngle -= (data.RotationInput.y * _config.RotationSpeed);
            _targetVerticalAngle = Mathf.Clamp(_targetVerticalAngle, _config.MinVerticalAngle, _config.MaxVerticalAngle);
            Quaternion verticalRot = Quaternion.Euler(_targetVerticalAngle, 0, 0);
            Quaternion targetRotation = planarRot * verticalRot;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, delta * _config.RotationSharpness);
            
            PostTick(delta);
        }

        protected virtual void PostTick(float delta)
        {
            
        }

        protected virtual void ProcessCameraZoomInput(float input)
        {
            
        }

        #endregion TICK
        
        
        #region VIEW STATE MOVE/LOOK

        /// <summary>
        /// Returns move and look vector values based on camera mode
        /// </summary>
        /// <param name="moveInput"></param>
        /// <returns></returns>
        internal Tuple<Vector3, Vector3> GetMoveAndLookVectors(Vector3 moveInput)
        {
            // Move vector
            Vector3 camPlanarDirection = GetCameraPlanarDirection(transform.rotation);
            Vector3 moveVector = 
                _kcc.CurrentState == ControllerKCCStateKey.Climbing ? 
                moveInput : 
                GetCameraPlanarRotation(camPlanarDirection) * moveInput;
            
            // Look vector
            Vector3 lookVector = _operator.ViewState switch
            {
                KCCCameraViewState.FirstPerson => camPlanarDirection,
                KCCCameraViewState.ThirdPerson => moveVector.normalized,
                _ => Vector3.zero
            };

            return new Tuple<Vector3, Vector3>(moveVector, lookVector);
        }
        
        private Vector3 GetCameraPlanarDirection(Quaternion cameraRotation)
        {
            // Calculate camera direction and rotation on the character plane
            Vector3 cameraPlanarDirection = Vector3.ProjectOnPlane(
                cameraRotation * Vector3.forward, 
                _kcc.Motor.CharacterUp).normalized;
            
            if (cameraPlanarDirection.sqrMagnitude == 0f)
            {
                cameraPlanarDirection = Vector3.ProjectOnPlane(
                    cameraRotation * Vector3.up, 
                    _kcc.Motor.CharacterUp).normalized;
            }

            return cameraPlanarDirection;
        }

        private Quaternion GetCameraPlanarRotation(Vector3 cameraPlanarDirection)
        {
            return Quaternion.LookRotation(cameraPlanarDirection, _kcc.Motor.CharacterUp);
        }

        #endregion VIEW STATE MOVE/LOOK
    }
}