using System.Collections.Generic;
using GalaxyGourd.Input;
using GalaxyGourd.Tick;
using KinematicCharacterController;
using UnityEngine;

namespace GalaxyGourd.KCC
{
    /// <summary>
    /// Class assigned to ANY KCC, AI or player-controlled
    /// </summary>
    [RequireComponent(typeof(KinematicCharacterMotor))]
    public class ControllerKCC : TickableBehaviour, IKinematicCharacterControllable
    {
        #region VARIABLES

        [Header("References")]
        [SerializeField] private Transform _meshRoot;
        [SerializeField] private Transform _cameraTargetFP;
        [SerializeField] private Transform _cameraTargetTP;
        [SerializeField] private List<Collider> _ignoredColliders = new List<Collider>();

        [Header("Config")]
        [SerializeField] private DataConfigKCC _config;
        
        public override string TickGroup => TickSettings.TickControllerHumanoid;
        public KinematicCharacterMotor Motor => _motor;
        public IKCCOperator Operator { get; set; }
        internal DataConfigKCC Config => _config;
        internal Transform MeshRoot => _meshRoot;
        internal float MaxStableMoveSpeed { get; set; }
        public Transform CameraTargetFP => _cameraTargetFP;
        public Transform CameraTargetTP => _cameraTargetTP;
        public ControllerKCCStateKey CurrentState => _currentState.Key;
        public IKCCAnimationProvider AnimatorProvider { get; private set; }
        public BonusOrientationMethod BonusOrientationMethod { get; set; }
        internal Vector3 LocalVelocity { get; private set; }
        internal KCCMovementCollisionType CurrentMovementCollision { get; set; } = KCCMovementCollisionType.None;

        private KinematicCharacterMotor _motor;
        private ControllerKCCState _currentState;
        private DataInputValuesControllerKCC _input;
        private readonly Collider[] _probedColliders = new Collider[8];
        private RaycastHit[] _probedHits = new RaycastHit[8];
        private Vector3 _moveInputVector;
        private Vector3 _lookInputVector;
        private Vector3 _internalVelocityAdd = Vector3.zero;
        private readonly List<ControllerKCCState> _states = new List<ControllerKCCState>();

        private static ControllerKCCStateKey _defaultStatus => ControllerKCCStateKey.Ungrounded;

        #endregion VARIABLES
        
        
        #region INTIIALIZATION

        private void Awake()
        {
            // Create and associate motor
            _motor = gameObject.GetComponent<KinematicCharacterMotor>();
            _motor.CharacterController = this;
            
            // Get animator
            if (TryGetComponent(out IKCCAnimationProvider provider))
            {
                AnimatorProvider = provider;
            }
            
            // Create states
            CreateControllerStates();
            _currentState = GetState(_defaultStatus);
            BonusOrientationMethod = Config.InitialBonusOrientationMethod;
        }

        private void OnEnable()
        {
            Ticker.Register(this);
        }

        private void OnDisable()
        {
            Ticker.Unregister(this);
        }

        private void OnDestroy()
        {
            foreach (ControllerKCCState state in _states)
            {
                state.Cleanup();
            }
        }

        #endregion INTIIALIZATION
        
        
        #region INPUT

        void IInputReceiver<DataInputValuesControllerKCC>.ReceiveInput(DataInputValuesControllerKCC inputData, float delta)
        {
            _input = inputData;
            
            // The current state should look for substate changes based on user input
            _currentState?.ForwardInputValues(_input);
        }

        #endregion INPUT


        #region TICK

        public override void Tick(float delta)
        {
            // Set move and look vectors
            _moveInputVector = _input.MoveVector;
            _lookInputVector = _input.LookDirection;
        }

        #endregion TICK
        
        
        #region STATE
        
        /// <summary>
        /// 
        /// </summary>
        private void CreateControllerStates()
        {
            _states.Add(new ControllerKCCStateGrounded(this));
            _states.Add(new ControllerHumanoidStateUngrounded(this));
            _states.Add(new ControllerHumanoidStateClimbing(this));
            _states.Add(new ControllerHumanoidStateSwimming(this));
        }
        
        /// <summary>
        /// Transitions between two controller states based on status
        /// </summary>
        /// <param name="fromStatus">The status we are leaving</param>
        /// <param name="toStatus">The status we are entering</param>
        /// <param name="transitionData">Optional data to pass into the entering state</param>
        internal void TransitionFromToState(
            ControllerKCCStateKey fromStatus,
            ControllerKCCStateKey toStatus,
            string transitionData = null)
        {
            ControllerKCCState from = GetState(fromStatus);
            ControllerKCCState to = GetState(toStatus);

            from.TransitionFromState(toStatus);
            to.TransitionToState(fromStatus, transitionData);
            
            _currentState = to;
        }

        private ControllerKCCState GetState(ControllerKCCStateKey status)
        {
            ControllerKCCState state = null;
            foreach (ControllerKCCState thisState in _states)
            {
                if (thisState.Key == status)
                {
                    state = thisState;
                    break;
                }
            }
            
            return state;
        }

        #endregion STATE

        
        #region MOTOR CALLBACKS
        
        void ICharacterController.BeforeCharacterUpdate(float deltaTime)
        {
            // Reset movement collision for this update cycle
            CurrentMovementCollision = KCCMovementCollisionType.None;
            
            _currentState?.BeforeCharacterUpdate(deltaTime);
        }
        
        void ICharacterController.PostGroundingUpdate(float deltaTime)
        {
            // Handle landing and leaving ground
            if (_motor.GroundingStatus.FoundAnyGround && !_motor.LastGroundingStatus.FoundAnyGround)
            {
                _currentState?.OnCharacterGrounded();
            }
            else if (!_motor.GroundingStatus.FoundAnyGround && _motor.LastGroundingStatus.FoundAnyGround)
            {
                _currentState?.OnCharacterUngrounded();
            }
        }
        
        void ICharacterController.UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            // In first person, the rotation is handled directly by the camera (VirtualCameraKCCFP) so we only update for third
            if (Operator.ViewState == KCCCameraViewState.ThirdPerson)
            {
                _currentState?.UpdateRotation(ref currentRotation, _lookInputVector, deltaTime);
            }
        }

        void ICharacterController.UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            // Take into account additive velocity
            if (_internalVelocityAdd.sqrMagnitude > 0f)
            {
                currentVelocity += _internalVelocityAdd;
                _internalVelocityAdd = Vector3.zero;
            }
            
            _currentState?.UpdateVelocity(ref currentVelocity, _moveInputVector, deltaTime);
        }
        
        void ICharacterController.AfterCharacterUpdate(float delta)
        {
            LocalVelocity = _motor.transform.InverseTransformDirection(_motor.Velocity);
            _currentState?.AfterCharacterUpdate(delta);

            // Update animator properties
            AnimatorProvider?.UpdateAnimationProperties(delta, this);
        }
        
        void ICharacterController.OnDiscreteCollisionDetected(Collider hitCollider)
        {
            _currentState?.OnDiscreteCollisionDetected(hitCollider);
        }
        
        void ICharacterController.OnGroundHit(
            Collider hitCollider, 
            Vector3 hitNormal, 
            Vector3 hitPoint, 
            ref HitStabilityReport hitStabilityReport)
        {
            _currentState?.OnGroundHit(hitCollider, hitNormal, hitPoint, ref hitStabilityReport);
        }
        
        void ICharacterController.OnMovementHit(
            Collider hitCollider, 
            Vector3 hitNormal, 
            Vector3 hitPoint, 
            ref HitStabilityReport hitStabilityReport)
        {
            _currentState?.OnMovementHit(hitCollider, hitNormal, hitPoint, ref hitStabilityReport);
        }
        
        void ICharacterController.ProcessHitStabilityReport(
            Collider hitCollider,
            Vector3 hitNormal,
            Vector3 hitPoint,
            Vector3 atCharacterPosition,
            Quaternion atCharacterRotation,
            ref HitStabilityReport hitStabilityReport)
        {
            // Some objects may need to receive collision information about this event
            if (hitCollider.TryGetComponent(out IKCCCollisionReceivable receivable))
            {
                receivable.ReceiveKCCCollision(hitCollider, 
                    hitNormal,
                    hitPoint,
                    atCharacterPosition,
                    atCharacterRotation,
                    hitStabilityReport);
            }
            
            _currentState?.ProcessHitStabilityReport(
                hitCollider, 
                hitNormal,
                hitPoint,
                atCharacterPosition,
                atCharacterRotation,
                ref hitStabilityReport);
        }

        bool ICharacterController.IsColliderValidForCollisions(Collider coll)
        {
            return !_ignoredColliders.Contains(coll);
        }
        
        #endregion MOTOR CALLBACKS
        

        #region UTILITY

        /// <summary>
        /// Attempts to set new dimensions for the capsule collider - if there's a collision conflict,
        /// the capsule will be returned to it's original dimensions
        /// </summary>
        /// <returns>TRUE if the change was successful</returns>
        internal bool TryApplyNewMotorCapsuleDimensions(float height, float yOffset)
        {
            float cachedHeight = Motor.Capsule.height;
            float cachedOffset = Motor.Capsule.center.y;
            
            Motor.SetCapsuleDimensions( 0.5f, height, yOffset);

            if (Motor.CharacterOverlap(
                    Motor.TransientPosition,
                    Motor.TransientRotation,
                    _probedColliders,
                    Motor.CollidableLayers,
                    QueryTriggerInteraction.Ignore) > 0)
            {
                // We can't resolve the new dimensions, as something is in the way. Return to the original dimensions
                Motor.SetCapsuleDimensions( 0.5f, cachedHeight, cachedOffset);
                
                return false;
            }

            return true;
        }

        #endregion UTILITY
    }
}