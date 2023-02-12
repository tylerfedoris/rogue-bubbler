using System;
using UnityEngine;

namespace _Project.Code.Scripts
{
    public class GameManager : MonoBehaviour
    {
        public static event Action OnRestart;
        
        [SerializeField] private GameObject _gameOverPanel;

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
            FailureBoundary.OnGameOver += HandleGameOver;
        }

        private void OnDisable()
        {
            FailureBoundary.OnGameOver -= HandleGameOver;
        }

        private void HandleGameOver()
        {
            _gameOverPanel.SetActive(true);
        }

        private void Restart()
        {
            OnRestart?.Invoke();
        }
    }
}
