using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Code.Scripts
{
    public class InputManager : MonoBehaviour
    {
        public static event Action<InputValue> OnInputRotate;
        public static event Action OnInputLaunch;
    
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        private void OnEnable()
        {
            FailureBoundary.OnGameOver += SwitchInputToUI;
        }

        private void OnDisable()
        {
            FailureBoundary.OnGameOver -= SwitchInputToUI;
        }

        private void SwitchInputToUI()
        {
        
        }

        private void SwitchInputToLauncher()
        {
            
        }

        private void OnRotate(InputValue value)
        {
            OnInputRotate?.Invoke(value);
        }

        private void OnLaunch()
        {
            OnInputLaunch?.Invoke();
        }
    }
}
