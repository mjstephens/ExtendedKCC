using UnityEngine;

namespace GalaxyGourd.KCC
{
    /// <summary>
    /// Default animation implementation. Overwrite or replace for custom functionality
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class DefaultKCCAnimator : MonoBehaviour
    {
        #region VARIABLES
        
        private Animator _animator;
        private IKCCAnimationProvider _provider;
        
        private static readonly int ForwardSpeed = Animator.StringToHash("ForwardSpeed");
        private static readonly int VerticalSpeed = Animator.StringToHash("VerticalSpeed");
        private static readonly int LateralSpeed = Animator.StringToHash("LateralSpeed");
        private static readonly int TurningSpeed = Animator.StringToHash("TurningSpeed");
        private static readonly int SpeedMultiplier = Animator.StringToHash("SpeedMultiplier");
        private static readonly int RightFoot = Animator.StringToHash("OnRightFoot");
        private static readonly int Strafe = Animator.StringToHash("Strafe");
        private static readonly int Ungrounded = Animator.StringToHash("Ungrounded");
        private static readonly int Jumped = Animator.StringToHash("Jumped");
        private static readonly int Fall = Animator.StringToHash("Fall");
        
        private static readonly int LocomotionIdle = Animator.StringToHash("LocomotionIdle");
        private static readonly int LocomotionWalking = Animator.StringToHash("LocomotionWalking");
        private static readonly int LocomotionRunning = Animator.StringToHash("LocomotionRunning");
        private static readonly int LocomotionSprinting = Animator.StringToHash("LocomotionSprinting");
        private const string k_LandState = "Land";

        #endregion VARIABLES


        #region INITIALIZATION

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public void Init(IKCCAnimationProvider provider)
        {
            _provider = provider;
            if (_provider == null)
            {
                Destroy(this);
            }
            
            _provider.GroundedLocomotionTypeChanged = OnGroundedLocomotionTypeChanged;
            _provider.GroundedMoveTypeChanged = OnGroundedMoveTypeChanged;
            _provider.GroundedPostureChanged = OnGroundedPostureChanged;
            _provider.DoLand = OnLand;
        }

        #endregion INITIALIZATION


        #region TICK

        public void Tick(float delta)
        {
            if (_provider == null)
                return;
            
            SetAnimatorProperties(delta);
        }

        private void SetAnimatorProperties(float delta) 
        {
            _animator.SetFloat(ForwardSpeed, _provider.ForwardSpeed);
            _animator.SetFloat(VerticalSpeed, _provider.VerticalSpeed);
            _animator.SetFloat(LateralSpeed, _provider.LateralSpeed);
            _animator.SetFloat(TurningSpeed, _provider.TurningSpeed);
            
            _animator.SetFloat(SpeedMultiplier, 1);
            _animator.SetBool(RightFoot, _provider.OnRightFoot);
            _animator.SetBool(Strafe, _provider.Strafe);
            _animator.SetBool(Ungrounded, _provider.Ungrounded);

            if (_provider.Jumped)
            {
                _animator.SetTrigger(Jumped);
            }
            else
            {
                _animator.ResetTrigger(Jumped);
            }
            
            if (_provider.Fall)
            {
                _animator.SetTrigger(Fall);
            }
            else
            {
                _animator.ResetTrigger(Fall);
            }
        }

        private void OnLand()
        {
            _animator.CrossFade(k_LandState, 0.05f);
        }

        #endregion TICK


        #region STATES

        private void OnGroundedLocomotionTypeChanged(KCCGroundedLocomotionType from, KCCGroundedLocomotionType to)
        {
            // _animator.SetBool(GetAnimStateForLocomotionType(from), false);
            // _animator.SetBool(GetAnimStateForLocomotionType(to), true);
            //

            // switch (to)
            // {
            //     case KCCGroundedLocomotionType.Idle: _animator.SetFloat(ForwardSpeed, 0); break;
            //     case KCCGroundedLocomotionType.Walk: _animator.SetFloat(ForwardSpeed, 1); break;
            //     case KCCGroundedLocomotionType.Run: _animator.SetFloat(ForwardSpeed, 2); break;
            //     case KCCGroundedLocomotionType.Sprint: _animator.SetFloat(ForwardSpeed, 3); break;
            // }
            
        }
        
        private void OnGroundedMoveTypeChanged(ControllerKCCStateGroundedMoveType from, ControllerKCCStateGroundedMoveType to)
        {
            
        }
        
        private void OnGroundedPostureChanged(ControllerKCCStateGroundedPosture from, ControllerKCCStateGroundedPosture to)
        {
            
        }
        
        #endregion STATE


        #region UTILITY

        private static int GetAnimStateForLocomotionType(KCCGroundedLocomotionType type)
        {
            return type switch
            {
                KCCGroundedLocomotionType.Idle => LocomotionIdle,
                KCCGroundedLocomotionType.Walk => LocomotionWalking,
                KCCGroundedLocomotionType.Run => LocomotionRunning,
                KCCGroundedLocomotionType.Sprint => LocomotionSprinting,
                _ => LocomotionIdle
            };
        }

        #endregion UTILITY
    }
}