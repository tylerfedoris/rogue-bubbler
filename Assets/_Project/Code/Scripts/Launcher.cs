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

    private float _rotateValue = 0f;
    private bool _isLaunching = false;
    private GameObject _currentBubble;
    private Rigidbody2D _bubbleRigidBody;
    private Transform _launcherTransform;
    private Vector2 _prevLaunchDirection;
    private Vector2 _launchDirection;
    private LineRenderer _lineRenderer;
    
    // Start is called before the first frame update
    private void Start()
    {
        _launcherTransform = transform;
        _lineRenderer = GetComponent<LineRenderer>();
        _launchDirection = _launcherTransform.up;
        SpawnNextBubble();
    }

    // Update is called once per frame
    private void Update()
    {
        RotateByZ(_rotateValue * _rotateSpeed * Time.deltaTime);

        if (!_isLaunching)
        {
            _prevLaunchDirection = _launchDirection;
            _launchDirection = _launcherTransform.up;
            if (_prevLaunchDirection != _launchDirection)
            {
                RenderAimIndicator();
            }
        }
        
        if (_isLaunching)
        {
            MoveBubbleForward();
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
        if (!_isLaunching && _bubbleRigidBody != null)
        {
            _isLaunching = true;
            _currentBubble.transform.parent = null;
        }
    }

    private void MoveBubbleForward()
    {
        Vector2 desiredPosition = _bubbleRigidBody.position + _launchDirection * (_launchSpeed * Time.deltaTime);
        _bubbleRigidBody.MovePosition(desiredPosition);
    }

    private void SpawnNextBubble()
    {
        _currentBubble = Instantiate(_bubblePrefab, _bubbleSlot);

        if (_currentBubble)
        {
            _bubbleRigidBody = _currentBubble.GetComponent<Rigidbody2D>();
        }
    }

    private void RenderAimIndicator()
    {
        var startingLaunchPosition = _bubbleSlot.transform.position;
        
        var collisionPoints = new List<Vector2>();
        GetCollisionPoints(startingLaunchPosition, collisionPoints);

        _lineRenderer.positionCount = collisionPoints.Count + 1;
        _lineRenderer.SetPosition(0, startingLaunchPosition);

        for (var i = 0; i < collisionPoints.Count; i++)
        {
            _lineRenderer.SetPosition(i + 1, collisionPoints[i]);
        }
    }

    private void GetCollisionPoints(Vector2 startingLaunchPosition, List<Vector2> collisionPoints)
    {
        var launchPosition = startingLaunchPosition;
        var hitResults = new List<RaycastHit2D>();
        var contactFilter = new ContactFilter2D();
        if (Physics2D.Raycast(startingLaunchPosition, _launchDirection, contactFilter, hitResults) <= 0)
        {
            return;
        }
        
        collisionPoints.Add(hitResults[0].point);
        var prevHit = hitResults[0];

        while (collisionPoints.Count <= _maxCollisionPoints && !prevHit.collider.CompareTag(_topBoundaryTag))
        {
            hitResults.Clear();
            var reflectionVector = Vector2.Reflect(prevHit.point - (Vector2)launchPosition, prevHit.normal).normalized;
            if (Physics2D.Raycast(prevHit.point, reflectionVector, contactFilter, hitResults) <= 0)
            {
                break;
            }

            RaycastHit2D validHit = new RaycastHit2D();
            foreach (var hit in hitResults)
            {
                if (!hit.collider.CompareTag(prevHit.collider.tag))
                {
                    validHit = hit;
                    break;
                }
            }

            collisionPoints.Add(validHit.point);
            launchPosition = prevHit.point;
            prevHit = validHit;
        }
    }
}
