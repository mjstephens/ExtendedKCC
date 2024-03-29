using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GalaxyGourd.KCC
{
    /// <summary>
    /// Dedicated class for listening to actions from a given input map.
    /// </summary>
    public abstract class ActionMapListener<T> : MonoBehaviour
        where T : struct
    {
        #region VARIABLES

        protected abstract string MapName { get; }
        protected abstract bool EnabledByDefault { get; }
        public Action<T> GetData;

        protected abstract T Data { get; }
        protected PlayerInput _input;
        protected string _currentControlScheme;
        private InputActionMap _actionMap;

        #endregion VARIABLES


        #region INITIALIZATION

        public virtual void Init(PlayerInput input)
        {
            _input = input;
            _actionMap = _input.actions.FindActionMap(MapName);
            OnControlsChanged(_input);
            
            // Enable as needed to start
            if (EnabledByDefault) _actionMap.Enable();
            else _actionMap.Disable();

            // Subscribe for input callbacks
            _input.onActionTriggered += OnActionTriggered;
            _input.onControlsChanged += OnControlsChanged;
        }

        protected virtual void OnEnable()
        {
            _actionMap?.Enable();
        }

        protected virtual void OnDisable()
        {
            if (!_input)
                return;
            
            // Unsub from input callbacks
            _input.onActionTriggered -= OnActionTriggered;
            _input.onControlsChanged -= OnControlsChanged;
            _actionMap?.Disable();
        }

        #endregion INITIALIZATION
        
        
        #region TICK
        
        public void Tick(float delta)
        {
            // Collect data
            OnTick(delta);
            
            // Transmit data
            GetData?.Invoke(Data);
            
            // Reset data
            ResetData();
        }

        protected virtual void OnTick(float delta)
        {
            
        }
        
        protected virtual void ResetData()
        {
            
        }
        
        #endregion TICK


        #region INPUT

        public void EnableMap(bool doEnable)
        {
            if (doEnable)
            {
                _actionMap.Enable();
            }
            else
            {
                _actionMap.Disable();
            }
        }
        
        protected virtual void OnActionTriggered(InputAction.CallbackContext callback)
        {
            
        }
        
        protected virtual void OnControlsChanged(PlayerInput input)
        {
            _currentControlScheme = input.currentControlScheme;
        }

        #endregion INPUT
    }
}