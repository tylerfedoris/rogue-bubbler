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

    // Start is called before the first frame update
    private void Start()
    {
        _launcherTransform = transform;
        _lineRenderer = GetComponent<LineRenderer>();
        _launchDirection = _launcherTransform.up;
        _collisionRadius = Bubble.BubbleScale / 2f;
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
        
        bool hitCellContainingBubble = false;
        int validHitIndex = GetValidHitIndex(hitResults, out hitCellContainingBubble);

        if (validHitIndex < 0)
        {
            return;
        }
        
        _collisionPoints.Add(hitResults[validHitIndex].point);
        RaycastHit2D prevHit = hitResults[validHitIndex];

        while (_collisionPoints.Count <= _maxCollisionPoints && !prevHit.collider.CompareTag(_topBoundaryTag) && !prevHit.collider.CompareTag(_bubbleTag) && !hitCellContainingBubble)
        {
            hitResults.Clear();
            var reflectionVector = Vector2.Reflect(prevHit.point - (Vector2)launchPosition, prevHit.normal).normalized;
            if (Physics2D.CircleCast(prevHit.point, _collisionRadius, reflectionVector, contactFilter, hitResults) <= 0)
            {
                break;
            }

            validHitIndex = GetValidHitIndex(hitResults, out hitCellContainingBubble, prevHit.collider.tag);

            if (validHitIndex < 0)
            {
                continue;
            }

            _collisionPoints.Add(hitResults[validHitIndex].point);
            launchPosition = prevHit.point;
            prevHit = hitResults[validHitIndex];
        }
    }

    private int GetValidHitIndex(List<RaycastHit2D> hits, out bool hitCellContainingBubble, string prevHitTag = "Untagged")
    {
        int validIndex = -1;
        hitCellContainingBubble = false;
        
        for (var i = 0; i < hits.Count; i++)
        {
            if (hits[i].collider.CompareTag(prevHitTag))
            {
                continue;
            }
            
            if (hits[i].collider.CompareTag(_cellTag))
            {
                if (i < hits.Count - 1)
                {
                    var gridCell = hits[i + 1].collider.gameObject.GetComponent<GridCell>();
                    if (gridCell && gridCell.Bubble)
                    {
                        hitCellContainingBubble = true;
                        validIndex = i;
                        break;
                    }
                }
            }
            else
            {
                validIndex = i;
                break;
            }
        }

        return validIndex;
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

        var bubble = _currentBubble.GetComponent<Bubble>();
        bubble.PlaceBubble();
        _isLaunching = false;
        _currentBubble = null;
        _launchBubbleCoroutineRunning = false;        
    }
}
