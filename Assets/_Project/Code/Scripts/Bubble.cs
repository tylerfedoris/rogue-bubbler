using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Code.Scripts
{
    public class Bubble : MonoBehaviour
    {
        [Serializable]
        public enum BubbleType
        {
            Blue,
            Red,
            Green,
            Purple,
            Blocker,
            Debug
        }

        [Serializable]
        private struct BubbleSprites
        {
            public Sprite Blue;
            public Sprite Red;
            public Sprite Green;
            public Sprite Purple;
            public Sprite Blocker;
            public Sprite Debug;
            public Color BlueColor;
            public Color RedColor;
            public Color GreenColor;
            public Color PurpleColor;
            public Color BlockerColor;
            public Color DebugColor;
        }

        [SerializeField] private float _blockerScale = 0.75f;
        [SerializeField] private BubbleType _bubbleType = BubbleType.Blue;
        [SerializeField] private BubbleSprites _bubbleSprites;
        [SerializeField] private List<Collider2D> _collidingObjects = new();

        public BubbleType BubbleTypeProperty
        {
            get => _bubbleType;
            set => _bubbleType = value;
        }

        // Start is called before the first frame update
        private void Start()
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            switch (_bubbleType)
            {
                case BubbleType.Blue:
                    spriteRenderer.sprite = _bubbleSprites.Blue;
                    spriteRenderer.color = _bubbleSprites.BlueColor;
                    break;
                case BubbleType.Red:
                    spriteRenderer.sprite = _bubbleSprites.Red;
                    spriteRenderer.color = _bubbleSprites.RedColor;
                    break;
                case BubbleType.Green:
                    spriteRenderer.sprite = _bubbleSprites.Green;
                    spriteRenderer.color = _bubbleSprites.GreenColor;
                    break;
                case BubbleType.Purple:
                    spriteRenderer.sprite = _bubbleSprites.Purple;
                    spriteRenderer.color = _bubbleSprites.PurpleColor;
                    break;
                case BubbleType.Blocker:
                    spriteRenderer.sprite = _bubbleSprites.Blocker;
                    spriteRenderer.color = _bubbleSprites.BlockerColor;
                    transform.localScale = new Vector3(_blockerScale, _blockerScale, _blockerScale);
                    break;           
                case BubbleType.Debug:
                    spriteRenderer.sprite = _bubbleSprites.Debug;
                    spriteRenderer.color = _bubbleSprites.DebugColor;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            _collidingObjects.Add(other);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            _collidingObjects.Remove(other);
        }
    }
}
