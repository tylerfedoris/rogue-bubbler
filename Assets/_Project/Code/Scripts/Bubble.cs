using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bubble : MonoBehaviour
{
    public const float BubbleScale = 0.5f;
    private const float _blockerScale = 0.4f;

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
    
    [SerializeField] private BubbleType _bubbleType = BubbleType.Blue;
    [SerializeField] private BubbleSprites _bubbleSprites;
    [SerializeField] private List<Collider2D> _collidingObjects = new();

    private Transform _transform;

    public BubbleType BubbleTypeProperty
    {
        get => _bubbleType;
        set => _bubbleType = value;
    }

    // Start is called before the first frame update
    private void Start()
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        _transform = transform;
        switch (_bubbleType)
        {
            case BubbleType.Blue:
                spriteRenderer.sprite = _bubbleSprites.Blue;
                spriteRenderer.color = _bubbleSprites.BlueColor;
                _transform.localScale = new Vector3(BubbleScale, BubbleScale, BubbleScale);
                break;
            case BubbleType.Red:
                spriteRenderer.sprite = _bubbleSprites.Red;
                spriteRenderer.color = _bubbleSprites.RedColor;
                _transform.localScale = new Vector3(BubbleScale, BubbleScale, BubbleScale);
                break;
            case BubbleType.Green:
                spriteRenderer.sprite = _bubbleSprites.Green;
                spriteRenderer.color = _bubbleSprites.GreenColor;
                _transform.localScale = new Vector3(BubbleScale, BubbleScale, BubbleScale);
                break;
            case BubbleType.Purple:
                spriteRenderer.sprite = _bubbleSprites.Purple;
                spriteRenderer.color = _bubbleSprites.PurpleColor;
                _transform.localScale = new Vector3(BubbleScale, BubbleScale, BubbleScale);
                break;
            case BubbleType.Blocker:
                spriteRenderer.sprite = _bubbleSprites.Blocker;
                spriteRenderer.color = _bubbleSprites.BlockerColor;
                _transform.localScale = new Vector3(_blockerScale, _blockerScale, _blockerScale);
                break;           
            case BubbleType.Debug:
                spriteRenderer.sprite = _bubbleSprites.Debug;
                spriteRenderer.color = _bubbleSprites.DebugColor;
                _transform.localScale = new Vector3(BubbleScale, BubbleScale, BubbleScale);
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
