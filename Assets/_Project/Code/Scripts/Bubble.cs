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
            Apple,
            Cherry,
            Orange,
            Pear,
            Peach,
            Blocker,
            Debug
        }

        [Serializable]
        private struct BubbleSprites
        {
            public Sprite Apple;
            public Sprite Cherry;
            public Sprite Orange;
            public Sprite Pear;
            public Sprite Peach;
            public Sprite Blocker;
            public Sprite Debug;
            public Color AppleColor;
            public Color CherryColor;
            public Color OrangeColor;
            public Color PearColor;
            public Color PeachColor;
            public Color BlockerColor;
            public Color DebugColor;
        }

        [SerializeField] private float _blockerScale = 0.75f;
        [SerializeField] private BubbleType _bubbleType = BubbleType.Apple;
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
                case BubbleType.Apple:
                    spriteRenderer.sprite = _bubbleSprites.Apple;
                    spriteRenderer.color = _bubbleSprites.AppleColor;
                    break;
                case BubbleType.Cherry:
                    spriteRenderer.sprite = _bubbleSprites.Cherry;
                    spriteRenderer.color = _bubbleSprites.CherryColor;
                    break;
                case BubbleType.Orange:
                    spriteRenderer.sprite = _bubbleSprites.Orange;
                    spriteRenderer.color = _bubbleSprites.OrangeColor;
                    break;
                case BubbleType.Pear:
                    spriteRenderer.sprite = _bubbleSprites.Pear;
                    spriteRenderer.color = _bubbleSprites.PearColor;
                    break;
                case BubbleType.Peach:
                    spriteRenderer.sprite = _bubbleSprites.Peach;
                    spriteRenderer.color = _bubbleSprites.PeachColor;
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
