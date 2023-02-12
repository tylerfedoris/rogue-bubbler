using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Code.Scripts
{
    public class InputManager : MonoBehaviour
    {
        public static event Action<InputValue> OnInputRotate;
        public static event Action OnInputLaunch;

        private const string _launcherActionMapName = "Launcher";
        private const string _uiActionMapName = "UI";

        private PlayerInput Input { get; set; }
        private static string LauncherActionMapName => _launcherActionMapName;
        private static string UIActionMapName => _uiActionMapName;

        // Start is called before the first frame update
        void Start()
        {
            Input = GetComponent<PlayerInput>();
        }

        private void OnEnable()
        {
            FailureBoundary.OnGameOver += SwitchInputToUI;
            GameManager.OnStartNewGame += SwitchInputToLauncher;
        }

        private void OnDisable()
        {
            FailureBoundary.OnGameOver -= SwitchInputToUI;
            GameManager.OnStartNewGame -= SwitchInputToLauncher;
        }

        private void SwitchInputToUI()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;

            if (Input)
            {
                Input.SwitchCurrentActionMap(UIActionMapName);   
            }
        }
        
        private void SwitchInputToLauncher()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            if (Input)
            {
                Input.SwitchCurrentActionMap(LauncherActionMapName);   
            }
        }

        // ReSharper disable once UnusedMember.Local
        private void OnRotate(InputValue value)
        {
            OnInputRotate?.Invoke(value);
        }

        // ReSharper disable once UnusedMember.Local
        private void OnLaunch()
        {
            OnInputLaunch?.Invoke();
        }
    }
}
