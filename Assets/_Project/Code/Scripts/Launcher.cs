using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Launcher : MonoBehaviour
{
    [SerializeField] private float _rotateSpeed = 100.0f;
    [SerializeField] private float _maxRotationDegrees = 60.0f;
    [SerializeField] private float _launchSpeed = 60.0f;
    [SerializeField] private Transform _bubbleSlot;
    [SerializeField] private GameObject _bubblePrefab;

    private float _rotateValue = 0f;
    private bool _isLaunching = false;
    private GameObject _currentBubble;
    private Rigidbody2D _bubbleRigidBody;
    private Transform _launcherTransform;
    private Vector2 _launchDirection;
    
    // Start is called before the first frame update
    private void Start()
    {
        _launcherTransform = transform;
        SpawnNextBubble();
    }

    // Update is called once per frame
    private void Update()
    {
        RotateByZ(_rotateValue * _rotateSpeed * Time.deltaTime);

        if (!_isLaunching)
        {
            _launchDirection = _launcherTransform.up;
            RenderAimIndicator();
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
        // List<RaycastHit2D> hit;
        // if ()
        // {
        //     
        // }
    }
}
