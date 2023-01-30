using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bubble : MonoBehaviour
{
    [Serializable]
    public enum BubbleType
    {
        Blue,
        Red,
        Green,
        Purple,
        Blocker
    }

    [Serializable]
    private struct BubbleSprites
    {
        public Sprite Blue;
        public Sprite Red;
        public Sprite Green;
        public Sprite Purple;
        public Sprite Blocker;
        public Color BlueColor;
        public Color RedColor;
        public Color GreenColor;
        public Color PurpleColor;
        public Color BlockerColor;
    }

    [SerializeField] private float _blockerScale = 0.75f;
    [SerializeField] private float _bubbleScale = 1.0f;
    [SerializeField] private BubbleType _bubbleType = BubbleType.Blue;
    [SerializeField] private BubbleSprites _bubbleSprites;

    public float BubbleScale => _bubbleScale;

    public BubbleType BubbleTypeProperty
    {
        get => _bubbleType;
        set => _bubbleType = value;
    }

    // Start is called before the first frame update
    private void Start()
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        var myTransform = transform;
        switch (_bubbleType)
        {
            case BubbleType.Blue:
                spriteRenderer.sprite = _bubbleSprites.Blue;
                spriteRenderer.color = _bubbleSprites.BlueColor;
                myTransform.localScale = new Vector3(_bubbleScale, _bubbleScale, _bubbleScale);
                break;
            case BubbleType.Red:
                spriteRenderer.sprite = _bubbleSprites.Red;
                spriteRenderer.color = _bubbleSprites.RedColor;
                myTransform.localScale = new Vector3(_bubbleScale, _bubbleScale, _bubbleScale);
                break;
            case BubbleType.Green:
                spriteRenderer.sprite = _bubbleSprites.Green;
                spriteRenderer.color = _bubbleSprites.GreenColor;
                myTransform.localScale = new Vector3(_bubbleScale, _bubbleScale, _bubbleScale);
                break;
            case BubbleType.Purple:
                spriteRenderer.sprite = _bubbleSprites.Purple;
                spriteRenderer.color = _bubbleSprites.PurpleColor;
                myTransform.localScale = new Vector3(_bubbleScale, _bubbleScale, _bubbleScale);
                break;
            case BubbleType.Blocker:
                spriteRenderer.sprite = _bubbleSprites.Blocker;
                spriteRenderer.color = _bubbleSprites.BlockerColor;
                myTransform.localScale = new Vector3(_blockerScale, _blockerScale, _blockerScale);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    // Update is called once per frame
    private void Update()
    {
        
    }
}
