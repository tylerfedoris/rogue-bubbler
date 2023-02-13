using System;
using TMPro;
using UnityEngine;

namespace _Project.Code.Scripts
{
    public class GameManager : MonoBehaviour
    {
        public static event Action OnStartNewGame;
        public static event Action<int> OnStartLevel;

        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _scoreText;

        // Start is called before the first frame update
        void Start()
        {
            StartNewGame();
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        private void OnEnable()
        {
            FailureBoundary.OnGameOver += HandleGameOver;
            GridSystem.OnLevelCompleted += HandleLevelCompleted;
        }

        private void OnDisable()
        {
            FailureBoundary.OnGameOver -= HandleGameOver;
            GridSystem.OnLevelCompleted -= HandleLevelCompleted;
        }

        private void HandleGameOver()
        {
            _gameOverPanel.SetActive(true);
        }

        private void StartNewGame()
        {
            _levelText.text = "1";
            OnStartNewGame?.Invoke();
        }

        private void HandleLevelCompleted(int levelCompleted)
        {
            int nextLevel = levelCompleted + 1;
            
            OnStartLevel?.Invoke(nextLevel);
            
            _levelText.text = nextLevel.ToString();
        }

        // ReSharper disable once UnusedMember.Global
        public void Restart()
        {
            _gameOverPanel.SetActive(false);
            StartNewGame();
        }
    }
}
