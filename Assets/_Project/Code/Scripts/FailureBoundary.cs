using System;
using UnityEngine;

namespace _Project.Code.Scripts
{
    public class FailureBoundary : MonoBehaviour
    {
        public static event Action OnGameOver;

        private void OnTriggerEnter2D(Collider2D other)
        {
            OnGameOver?.Invoke();
        }
    }
}
