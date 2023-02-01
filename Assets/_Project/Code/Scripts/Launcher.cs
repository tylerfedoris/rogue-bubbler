using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Launcher : MonoBehaviour
{
    [SerializeField] private float _rotateSpeed = 100.0f;
    [SerializeField] private float _maxRotationDegrees = 60.0f;
    [SerializeField] private float _launchSpeed = 60.0f;
    [SerializeField] private int _maxCollisionPoints = 10;
    [SerializeField] private Transform _bubbleSlot;
    [SerializeField] private GameObject _bubblePrefab;
    [SerializeField] private string _topBoundaryTag = "TopBoundary";
    [SerializeField] private string _bubbleTag = "Bubble";
    [SerializeField] private string _cellTag = "Cell";

    private float _rotateValue = 0f;
    private bool _isLaunching = false;
    private GameObject _currentBubble;
    private Rigidbody2D _bubbleRigidBody;
    private Collider2D _bubbleCollider;
    private Transform _launcherTransform;
    private Vector2 _prevLaunchDirection;
    private Vector2 _launchDirection;
    private LineRenderer _lineRenderer;
    private float _collisionRadius;
    private List<Vector2> _collisionPoints;
    private float _elapsedLaunchTime;
    private Transform _bubbleSlotTransform;
    private bool _launchBubbleCoroutineRunning;
    private GameObject _previewBubble;
    private GridCell _targetGridCell;

    // Start is called before the first frame update
    private void Start()
    {
        _launcherTransform = transform;
        _lineRenderer = GetComponent<LineRenderer>();
        _launchDirection = _launcherTransform.up;
        _collisionRadius = Bubble.BubbleScale / 4f;
        _bubbleSlotTransform = _bubbleSlot.transform;
        SpawnNextBubble();
    }

    // Update is called once per frame
    private void Update()
    {
        RotateByZ(_rotateValue * _rotateSpeed * Time.deltaTime);

        if (_isLaunching && _currentBubble && !_launchBubbleCoroutineRunning)
        {
            _lineRenderer.enabled = false;
            StartCoroutine(LaunchBubbleCoroutine());
        }
        
        if (!_isLaunching)
        {
            if (!_currentBubble)
            {
                SpawnNextBubble();
            }
            _lineRenderer.enabled = true;
            _prevLaunchDirection = _launchDirection;
            _launchDirection = _launcherTransform.up;
            if (_prevLaunchDirection != _launchDirection)
            {
                if (_targetGridCell)
                {
                    _targetGridCell = null;
                }
                if (_previewBubble)
                {
                    Destroy(_previewBubble);
                }
                RenderAimIndicator();
            }
        }
    }

    private void RotateByZ(float angle)
    {
        float desiredAngle = Mathf.Repeat(transform.eulerAngles.z + angle + 180, 360) - 180;

        transform.eulerAngles =
            new Vector3(0f, 0f, Mathf.Clamp(desiredAngle, -_maxRotationDegrees, _maxRotationDegrees));
    }

    private void OnRotate(InputValue value)
    {
        _rotateValue = -value.Get<float>();
    }

    private void OnLaunch()
    {
        if (!_isLaunching)
        {
            _isLaunching = true;
            _currentBubble.transform.parent = null;
        }
    }

    private void SpawnNextBubble()
    {
        _currentBubble = Instantiate(_bubblePrefab, _bubbleSlot);

        if (_currentBubble)
        {
            _currentBubble.transform.localPosition = Vector3.zero;
            _bubbleRigidBody = _currentBubble.GetComponent<Rigidbody2D>();
            
            _bubbleCollider = _currentBubble.GetComponent<Collider2D>();
            _bubbleCollider.enabled = false;
        }
    }

    private void RenderAimIndicator()
    {
        var startingLaunchPosition = _bubbleSlotTransform.position;
        
        _collisionPoints = new List<Vector2>();
        GetCollisionPoints(startingLaunchPosition);

        _lineRenderer.positionCount = _collisionPoints.Count + 1;
        _lineRenderer.SetPosition(0, startingLaunchPosition);

        for (var i = 0; i < _collisionPoints.Count; i++)
        {
            _lineRenderer.SetPosition(i + 1, _collisionPoints[i]);
        }
    }

    private void GetCollisionPoints(Vector2 startingLaunchPosition)
    {
        var launchPosition = startingLaunchPosition;
        var hitResults = new List<RaycastHit2D>();
        var contactFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = ~(1 << 2),
            useTriggers = true
        };
        if (Physics2D.CircleCast(startingLaunchPosition, _collisionRadius, _launchDirection, contactFilter, hitResults) <= 0)
        {
            return;
        }
        
        int validHitIndex = GetValidHitIndex(hitResults);

        if (validHitIndex < 0)
        {
            return;
        }

        _collisionPoints.Add(_targetGridCell ? _targetGridCell.transform.position : hitResults[validHitIndex].point);
        var prevHit = hitResults[validHitIndex];

        while (!_targetGridCell && _collisionPoints.Count <= _maxCollisionPoints && !prevHit.collider.CompareTag(_topBoundaryTag) && !prevHit.collider.CompareTag(_bubbleTag))
        {
            hitResults.Clear();
            var reflectionVector = Vector2.Reflect(prevHit.point - (Vector2)launchPosition, prevHit.normal).normalized;
            if (Physics2D.CircleCast(prevHit.point, _collisionRadius, reflectionVector, contactFilter, hitResults) <= 0)
            {
                break;
            }

            validHitIndex = GetValidHitIndex(hitResults, prevHit.collider.tag);

            if (validHitIndex < 0)
            {
                continue;
            }

            if (_targetGridCell)
            {
                _collisionPoints.Add(_targetGridCell.transform.position);
                continue;
            }

            _collisionPoints.Add(hitResults[validHitIndex].point);
            launchPosition = prevHit.point;
            prevHit = hitResults[validHitIndex];
        }
    }

    private int GetValidHitIndex(IReadOnlyList<RaycastHit2D> hits, string prevHitTag = "Untagged")
    {
        for (var i = 0; i < hits.Count; i++)
        {
            if (hits[i].collider.CompareTag(prevHitTag))
            {
                continue;
            }
            
            if (hits[i].collider.CompareTag(_cellTag))
            {
                var hitGridCell = hits[i].collider.gameObject.GetComponent<GridCell>();
                if (!hitGridCell || !hitGridCell.Bubble)
                {
                    continue;
                }
                
                var closestGridCell = FindClosestEmptyCell(hitGridCell, hits[i].point);
                    
                if (!closestGridCell)
                {
                    throw new Exception("An error occurred when attempting to find an empty grid cell in the hit results.");
                }
                    
                _previewBubble = Instantiate(_bubblePrefab, closestGridCell.transform);
                _previewBubble.GetComponent<Bubble>().BubbleTypeProperty = Bubble.BubbleType.Debug;
                _targetGridCell = closestGridCell;
                return i;
            }
            return i;
        }

        return -1;
    }

    private GridCell FindClosestEmptyCell(GridCell targetCell, Vector2 collisionPoint)
    {
        GridCell closestCell = null;
        var closestDistance = float.MaxValue;
        
        foreach (var cell in targetCell.ConnectedCells)
        {
            if (cell.Bubble)
            {
                continue;
            }
            
            float distance = Vector2.Distance(cell.transform.position, collisionPoint);
            if (distance < closestDistance)
            {
                closestCell = cell;
                closestDistance = distance;
            }
        }

        return closestCell;
    }

    IEnumerator LaunchBubbleCoroutine()
    {
        _launchBubbleCoroutineRunning = true;
        _bubbleCollider.enabled = true;
        for (var i = 0; i < _collisionPoints.Count; i++)
        {
            var distanceTraveled = 0f;
            var startPosition = i == 0 ? (Vector2)_bubbleSlotTransform.position : _collisionPoints[i - 1];
            var endPosition = _collisionPoints[i];
            var travelDistance = Vector2.Distance(startPosition, endPosition);
            while (distanceTraveled < travelDistance)
            {
                var movePosition = Vector2.Lerp(startPosition, endPosition, distanceTraveled / travelDistance);
                _bubbleRigidBody.MovePosition(movePosition);
                distanceTraveled += _launchSpeed * Time.deltaTime;

                yield return null;
            }
            
            _bubbleRigidBody.MovePosition(endPosition);
        }

        PlaceBubble();
    }

    private void PlaceBubble()
    {
        if (!_currentBubble)
        {
            throw new Exception("Bubble was missing when attempting to place it in target grid cell.");
        }
        
        if (!_targetGridCell)
        {
            throw new Exception("Target cell was missing when attempting to place bubble.");
        }

        _targetGridCell.Bubble = _currentBubble;
        _currentBubble.transform.parent = _targetGridCell.transform;
        _currentBubble.transform.localPosition = Vector3.zero;

        _isLaunching = false;
        _currentBubble = null;
        _launchBubbleCoroutineRunning = false;     
    }
}
