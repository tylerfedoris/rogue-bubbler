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
    [SerializeField] private List<Collider2D> _collidingObjects = new();

    public float BubbleScale => _bubbleScale;

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
                _transform.localScale = new Vector3(_bubbleScale, _bubbleScale, _bubbleScale);
                break;
            case BubbleType.Red:
                spriteRenderer.sprite = _bubbleSprites.Red;
                spriteRenderer.color = _bubbleSprites.RedColor;
                _transform.localScale = new Vector3(_bubbleScale, _bubbleScale, _bubbleScale);
                break;
            case BubbleType.Green:
                spriteRenderer.sprite = _bubbleSprites.Green;
                spriteRenderer.color = _bubbleSprites.GreenColor;
                _transform.localScale = new Vector3(_bubbleScale, _bubbleScale, _bubbleScale);
                break;
            case BubbleType.Purple:
                spriteRenderer.sprite = _bubbleSprites.Purple;
                spriteRenderer.color = _bubbleSprites.PurpleColor;
                _transform.localScale = new Vector3(_bubbleScale, _bubbleScale, _bubbleScale);
                break;
            case BubbleType.Blocker:
                spriteRenderer.sprite = _bubbleSprites.Blocker;
                spriteRenderer.color = _bubbleSprites.BlockerColor;
                _transform.localScale = new Vector3(_blockerScale, _blockerScale, _blockerScale);
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
