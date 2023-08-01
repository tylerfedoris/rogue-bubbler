using System;
using TMPro;
using UnityEngine;

namespace _Project.Code.Scripts
{
    public class GameManager : MonoBehaviour
    {
        public static event Action OnStartNewGame;
        public static event Action<int> OnStartLevel;

        [SerializeField] private int _maxBubbleChain = 17;
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _scoreText;

        public int Level { get; private set; }

        private int Score { get; set; }

        // Start is called before the first frame update
        private void Start()
        {
            StartNewGame();
        }

        private void OnEnable()
        {
            FailureBoundary.OnGameOver += HandleGameOver;
            GridSystem.OnLevelCompleted += HandleLevelCompleted;
            GridSystem.OnBubblesPopped += HandleBubblesPopped;
        }

        private void OnDisable()
        {
            FailureBoundary.OnGameOver -= HandleGameOver;
            GridSystem.OnLevelCompleted -= HandleLevelCompleted;
            GridSystem.OnBubblesPopped -= HandleBubblesPopped;
        }

        private void HandleGameOver()
        {
            _gameOverPanel.SetActive(true);
        }

        private void StartNewGame()
        {
            Level = 1;
            _levelText.text = Level.ToString();
            
            Score = 0;
            _scoreText.text = Score.ToString();
            
            OnStartNewGame?.Invoke();
        }

        private void HandleLevelCompleted()
        {
            Level++;
            
            OnStartLevel?.Invoke(Level);
            
            _levelText.text = Level.ToString();
        }

        private void HandleBubblesPopped(int numBubblesPopped)
        {
            if (numBubblesPopped == 0)
            {
                return;
            }
            
            int pointsScored = CalculateScore(numBubblesPopped);
            Score += pointsScored;
            _scoreText.text = Score.ToString();
        }

        private int CalculateScore(int numBubblesPopped)
        {
            return (int) Mathf.Pow(2, Mathf.Min(numBubblesPopped, _maxBubbleChain));
        }

        // ReSharper disable once UnusedMember.Global
        public void Restart()
        {
            _gameOverPanel.SetActive(false);
            StartNewGame();
        }
    }
}
