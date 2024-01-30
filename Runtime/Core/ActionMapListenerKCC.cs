using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GalaxyGourd.KCC
{
    public class ActionMapListenerKCC : ActionMapListener<DataInputValuesControllerKCC>
    {
        #region VARIABLES

        protected override DataInputValuesControllerKCC Data => _data;
        protected override string MapName => "KCC";
        protected override bool EnabledByDefault => true;

        private float _cameraScrollValue;
        private Vector2 _lookInputVector;
        private float _moveAxisForward;
        private float _moveAxisRight;
        private InputAction _actionMove;
        
        private InputAction _inputMove;
        private InputAction _inputLook;
        private InputAction _inputJump;
        private InputAction _inputRun;
        private InputAction _inputWalk;
        private InputAction _inputCrouch;
        private InputAction _inputViewToggle;
        private InputAction _inputScroll;
        private const string CONST_actionMoveName = "Move";
        private const string CONST_actionLookName = "Look";
        private const string CONST_actionJumpName = "Jump";
        private const string CONST_actionRunName = "Sprint";
        private const string CONST_actionWalkName = "Walk";
        private const string CONST_actionCrouchName = "Crouch";
        private const string CONST_actionViewToggleName = "ViewToggle";
        private const string CONST_actionScrollName = "Scroll";

        private BoolAction _actionJump;
        private BoolAction _actionSprint;
        private BoolAction _actionWalk;
        private BoolAction _actionCrouch;
        private BoolAction _actionViewToggle;

        private IKCCOperator _operator;
        private DataInputValuesControllerKCC _data;
        // Camera-related inputs we cache to pass to the camera director component
        private DataInputValuesKCCCamera _cameraInput;

        #endregion VARIABLES


        #region INITIALIZATION

        public override void Init(PlayerInput input)
        {
            base.Init(input);
            
            // Ready input actions
            InitializeActions();
        }

        public void SetReceivers(IKCCOperator op)
        {
            _operator = op;
        }
        
        private void InitializeActions()
        {
            // Cache input actions
            _inputMove = _input.actions.FindAction(CONST_actionMoveName);
            _inputLook = _input.actions.FindAction(CONST_actionLookName);
            _inputJump = _input.actions.FindAction(CONST_actionJumpName);
            _inputRun = _input.actions.FindAction(CONST_actionRunName);
            _inputWalk = _input.actions.FindAction(CONST_actionWalkName);
            _inputCrouch = _input.actions.FindAction(CONST_actionCrouchName);
            _inputViewToggle = _input.actions.FindAction(CONST_actionViewToggleName);
            _inputScroll = _input.actions.FindAction(CONST_actionScrollName);
            
            // Jump action
            _actionJump = new BoolAction();
            _actionJump.Initialize();
            _data.Jump = _actionJump;
            
            // Run action
            _actionSprint = new BoolAction();
            _actionSprint.Initialize();
            _data.Sprint = _actionSprint;
            
            // Walk action
            _actionWalk = new BoolAction();
            _actionWalk.Initialize();
            _data.Walk = _actionWalk;
            
            // Crouch action
            _actionCrouch = new BoolAction();
            _actionCrouch.Initialize();
            _data.Crouch = _actionCrouch;
            
            // View toggle action
            _actionViewToggle = new BoolAction();
            _actionViewToggle.Initialize();
            _cameraInput.ViewToggled = _actionViewToggle;
        }

        #endregion INITIALIZATION
        

        #region TICK
        
        protected override void OnTick(float delta)
        {
            base.OnTick(delta);
            
            // Process constant inputs
            OnProcessMove(_inputMove);
            OnProcessLook(_inputLook);
            OnProcessJump(_inputJump, delta);
            OnProcessRun(_inputRun, delta);
            OnProcessWalk(_inputWalk, delta);
            OnProcessCrouch(_inputCrouch, delta);
            OnProcessViewToggled(_inputViewToggle, delta);
            OnProcessScroll(_inputScroll, delta);
            
            //
            ProcessMoveLookVectors(delta);
            
            // Tell the operator our input is ready for this tick
            _operator.ReceiveInput(_data, delta);
            _operator.ReceiveInput(_cameraInput, delta);
            
            // Cleanup input
            _moveAxisForward = 0; 
            _moveAxisRight = 0;
            _cameraInput = new DataInputValuesKCCCamera()
            {
                RotationInput = Vector3.zero,
                ViewToggled = _actionViewToggle,
                ZoomInput = 0
            };
        }

        private void ProcessMoveLookVectors(float delta)
        {
            // Combine move directions into single vector
            Vector3 inputMoveVector = Vector3.ClampMagnitude(
                new Vector3(_moveAxisRight, 0f, _moveAxisForward), 
                1f);
            Tuple<Vector3, Vector3> moveLook = _operator.ActivePlayerCamera.GetMoveAndLookVectors(inputMoveVector);
            
            _data.MoveVector = moveLook.Item1;
            _data.LookDirection = moveLook.Item2;
            
            // Camera
            _cameraInput.RotationInput = _lookInputVector;
            _operator.CameraInputData = _cameraInput;
        }

        #endregion TICK
        
        
        #region ACTIONS

        private void OnProcessMove(InputAction context)
        {
            Vector2 val = context.ReadValue<Vector2>();
            _moveAxisForward = val.y;
            _moveAxisRight = val.x;
        }

        private void OnProcessLook(InputAction context)
        {
            HandleCameraInputLook(context.ReadValue<Vector2>());
        }

        private void OnProcessJump(InputAction context, float delta)
        {
            _actionJump.value = context.IsPressed();
            _actionJump.Update(delta);
            _data.Jump = _actionJump;
        }
        
        private void OnProcessRun(InputAction context, float delta)
        {
            _actionSprint.value = context.IsPressed();
            _actionSprint.Update(delta);
            _data.Sprint = _actionSprint;
        }
        
        private void OnProcessWalk(InputAction context, float delta)
        {
            _actionWalk.value = context.IsPressed();
            _actionWalk.Update(delta);
            _data.Walk = _actionWalk;
        }
        
        private void OnProcessCrouch(InputAction context, float delta)
        {
            _actionCrouch.value = context.IsPressed();
            _actionCrouch.Update(delta);
            _data.Crouch = _actionCrouch;
        }

        private void OnProcessViewToggled(InputAction context, float delta)
        {
            _actionViewToggle.value = context.IsPressed();
            _actionViewToggle.Update(delta);
            _cameraInput.ViewToggled = _actionViewToggle;
        }
        
        private void OnProcessScroll(InputAction context, float delta)
        {
            _cameraInput.ZoomInput = context.ReadValue<Vector2>().y;
        }
        
        #endregion ACTIONS
        
        
        #region CAMERA

        private void HandleCameraInputLook(Vector2 val)
        {
            // Create the look input vector for the camera
            _lookInputVector = new Vector3(val.x, val.y, 0f);
            _data.TurningLeft = val.x < 0;
        }

        #endregion CAMERA
    }
}