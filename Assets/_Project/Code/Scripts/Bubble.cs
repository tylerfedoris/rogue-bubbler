using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bubble : MonoBehaviour
{
    static public float BlockerScale = 0.4f;
    static public float BubbleScale = 0.5f;
    
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
                _transform.localScale = new Vector3(BlockerScale, BlockerScale, BlockerScale);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void PlaceBubble()
    {
        var gridCellCandidates = new List<GridCell>();
        foreach (var collidingObject in _collidingObjects)
        {
            var gridCell = collidingObject.GetComponent<GridCell>();
            if (gridCell && !gridCell.Bubble)
            {
                gridCellCandidates.Add(gridCell);
            }
        }

        if (gridCellCandidates.Count <= 0)
        {
            throw new Exception(
                "Something went wrong when attempting to place the bubble in the grid. No valid grid candidates were found.");
        }

        GridCell closestGridCell = null;
        var closestDistance = float.MaxValue;

        foreach (var gridCellCandidate in gridCellCandidates)
        {
            float distanceToCell = Vector2.Distance(_transform.position, gridCellCandidate.transform.position);
            if (distanceToCell < closestDistance)
            {
                closestDistance = distanceToCell;
                closestGridCell = gridCellCandidate;
            }
        }

        if (!closestGridCell)
        {
            return;
        }

        closestGridCell.Bubble = gameObject;
        _transform.parent = closestGridCell.transform;
        _transform.localPosition = Vector3.zero;
        _transform.GetComponent<Collider2D>().enabled = false;
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
