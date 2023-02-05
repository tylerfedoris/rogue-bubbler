using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Code.Scripts
{
    public class Launcher : MonoBehaviour
    {
        public static event Action<GridCell> OnBubblePlaced;
    
        private struct CollisionPoint
        {
            public Vector2 Point;
            public Vector2 Centroid;
        }

        [SerializeField] private bool _showPreviewBubble;
        [SerializeField] private bool _renderAimLine = true;
        [SerializeField] private bool _renderMaxAimLine = true;
        [SerializeField] private float _aimLineLength = 2f;
    
        [SerializeField] private bool _showDebugCollisionPoints = false;
        [SerializeField] private GameObject _debugCollisionPointPrefab;

        [SerializeField] private GridSystem _gridSystem;
        [SerializeField] private float _rotateSpeed = 100.0f;
        [SerializeField] private float _maxRotationDegrees = 60.0f;
        [SerializeField] private float _launchSpeed = 60.0f;
        [SerializeField] private int _maxCollisionPoints = 10;
        [SerializeField] private Transform _bubbleSlot;
        [SerializeField] private Transform _bubbleOnDeckSlot;
        [SerializeField] private GameObject _bubblePrefab;
        [SerializeField] private string _topBoundaryTag = "TopBoundary";
        [SerializeField] private string _cellTag = "Cell";

        private float _rotateValue;
        private bool _isLaunching;
        private bool _loadedNewBubble;
        private float _elapsedLaunchTime;
        private bool _launchBubbleCoroutineRunning;
    
        private GameObject _currentBubble;
        private GameObject _bubbleOnDeck;
        private Rigidbody2D _bubbleRigidBody;
    
        private Transform _launcherTransform;
        private LineRenderer _lineRenderer;
        private GridCell _targetGridCell;
    
        private Vector2 _prevLaunchDirection;
        private Vector2 _launchDirection;
    
        private float _collisionRadius;
        private List<CollisionPoint> _collisionPoints;

        private GameObject _previewBubble;
        private List<GameObject> _debugCollisionPoints = new();

        // Start is called before the first frame update
        private void Start()
        {
            _launcherTransform = transform;
            _lineRenderer = GetComponent<LineRenderer>();
            _launchDirection = _launcherTransform.up;
            _collisionRadius = Bubble.BubbleScale / 2f;
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
                    GetNextBubble();
                }
                _lineRenderer.enabled = true;
                _prevLaunchDirection = _launchDirection;
                _launchDirection = _launcherTransform.up;
                if (_prevLaunchDirection != _launchDirection || _loadedNewBubble)
                {
                    _loadedNewBubble = false;
                    if (_targetGridCell)
                    {
                        _targetGridCell = null;
                    }
                    if (_previewBubble)
                    {
                        Destroy(_previewBubble);
                    }
                    ClearDebugCollisionPoints();
                    DetermineAimPath();
                }
            }
        }

        private void ClearDebugCollisionPoints()
        {
            foreach (var point in _debugCollisionPoints)
            {
                Destroy(point);
            }
        
            _debugCollisionPoints.Clear();
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
            if (!_isLaunching && _currentBubble)
            {
                _isLaunching = true;
                _currentBubble.transform.parent = null;
            }
        }

        private void GetNextBubble()
        {
            if (!_bubbleOnDeck)
            {
                SpawnBubbleOnDeck();
            }

            _currentBubble = _bubbleOnDeck;
            _currentBubble.transform.parent = _bubbleSlot;
            _currentBubble.transform.localPosition = Vector3.zero;
            _bubbleRigidBody = _currentBubble.GetComponent<Rigidbody2D>();
            _loadedNewBubble = true;

            SpawnBubbleOnDeck();
        }

        private void SpawnBubbleOnDeck()
        {
            var validBubbleTypes = _gridSystem.BubbleTypesInPlay.Keys
                .Where(bubbleType => bubbleType != Bubble.BubbleType.Blocker && _gridSystem.BubbleTypesInPlay[bubbleType] > 0).ToList();
        
            var randomBubbleType = validBubbleTypes[UnityEngine.Random.Range(0, validBubbleTypes.Count)];

            _bubbleOnDeck = Instantiate(_bubblePrefab, _bubbleOnDeckSlot);
        
            if (!_bubbleOnDeck)
            {
                throw new Exception("There was a problem instantiating a new bubble on deck.");
            }
        
            _bubbleOnDeck.GetComponent<Bubble>().BubbleTypeProperty = randomBubbleType;
        }

        private void DetermineAimPath()
        {
            var startingLaunchPosition = _bubbleSlot.position;
        
            _collisionPoints = new List<CollisionPoint>();
            GetCollisionPoints(startingLaunchPosition);
        
            if (_showDebugCollisionPoints)
            {
                foreach (var collisionPoint in _collisionPoints)
                {
                    _debugCollisionPoints.Add(Instantiate(_debugCollisionPointPrefab, collisionPoint.Point, Quaternion.identity));
                }
            }

            if (!_renderAimLine)
            {
                return;
            }

            _lineRenderer.positionCount = _renderMaxAimLine ? _collisionPoints.Count + 1 : 2;
            _lineRenderer.SetPosition(0, startingLaunchPosition);

            if (_renderMaxAimLine)
            {
                for (var i = 0; i < _collisionPoints.Count; i++)
                {
                    _lineRenderer.SetPosition(i + 1, _collisionPoints[i].Point);
                }
            }
            else
            {
                _lineRenderer.SetPosition(1, startingLaunchPosition + _bubbleSlot.up * _aimLineLength);
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

            var point = hitResults[validHitIndex].point;
            Vector2 centroid = _targetGridCell ? _targetGridCell.transform.position : hitResults[validHitIndex].centroid;

            _collisionPoints.Add(new CollisionPoint{ Point = point, Centroid = centroid });
            var prevHit = hitResults[validHitIndex];

            while (!_targetGridCell && _collisionPoints.Count <= _maxCollisionPoints && !prevHit.collider.CompareTag(_topBoundaryTag))
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
                    _collisionPoints.Add(new CollisionPoint{Point = hitResults[validHitIndex].point, Centroid = _targetGridCell.transform.position});
                    continue;
                }

                _collisionPoints.Add(new CollisionPoint{ Point = hitResults[validHitIndex].point, Centroid = hitResults[validHitIndex].centroid });
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
                
                    var closestGridCell = FindClosestEmptyCell(hitGridCell, hits[i].centroid);
                    
                    if (!closestGridCell)
                    {
                        throw new Exception("An error occurred when attempting to find an empty grid cell in the hit results.");
                    }

                    if (_showPreviewBubble)
                    {
                        _previewBubble = Instantiate(_bubblePrefab, closestGridCell.transform);
                        _previewBubble.GetComponent<Bubble>().BubbleTypeProperty = Bubble.BubbleType.Debug;   
                    }
                
                    _targetGridCell = closestGridCell;
                }

                if (hits[i].collider.CompareTag(_topBoundaryTag))
                {
                    var closestGridCell = FindClosestEmptyCellInTopRow(hits[i].centroid);
                    
                    if (!closestGridCell)
                    {
                        throw new Exception("An error occurred when attempting to find an empty grid cell in the hit results.");
                    }
                
                    if (_showPreviewBubble)
                    {
                        _previewBubble = Instantiate(_bubblePrefab, closestGridCell.transform);
                        _previewBubble.GetComponent<Bubble>().BubbleTypeProperty = Bubble.BubbleType.Debug;   
                    }
                
                    _targetGridCell = closestGridCell;
                }
            
                return i;
            }

            return -1;
        }

        private GridCell FindClosestEmptyCell(GridCell targetCell, Vector2 collisionPoint, bool includeSelf = false)
        {
            GridCell closestCell = includeSelf ? targetCell : null;
            float closestDistance = includeSelf ? Vector2.Distance(targetCell.transform.position, collisionPoint) : float.MaxValue;
        
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

        private GridCell FindClosestEmptyCellInTopRow(Vector2 point)
        {
            var collisionResults = new List<Collider2D>();
            var contactFilter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = ~(1 << 2),
                useTriggers = true
            };

            if (Physics2D.OverlapCircle(point, _collisionRadius, contactFilter, collisionResults) <= 0)
            {
                return null;
            }

            foreach (var collision in collisionResults)
            {
                var gridCell = collision.GetComponent<Collider2D>().gameObject.GetComponent<GridCell>();
                if (gridCell && !gridCell.Bubble && gridCell.GridPosition.x == 0)
                {
                    return FindClosestEmptyCell(gridCell, point, true);
                }
            }

            return null;
        }

        IEnumerator LaunchBubbleCoroutine()
        {
            _launchBubbleCoroutineRunning = true;
            for (var i = 0; i < _collisionPoints.Count; i++)
            {
                var distanceTraveled = 0f;
                var startPosition = i == 0 ? (Vector2)_bubbleSlot.position : _collisionPoints[i - 1].Centroid;
                var endPosition = _collisionPoints[i].Centroid;
                float travelDistance = Vector2.Distance(startPosition, endPosition);
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
            _bubbleRigidBody = null;
            _launchBubbleCoroutineRunning = false;
        
            OnBubblePlaced?.Invoke(_targetGridCell);
        }
    }
}
