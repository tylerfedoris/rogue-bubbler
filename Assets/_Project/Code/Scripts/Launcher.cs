using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Launcher : MonoBehaviour
{
    [SerializeField] private float _rotateSpeed = 100.0f;
    [SerializeField] private float _maxRotationDegrees = 60.0f;
    [SerializeField] private GameObject[] _bubblePrefabs;

    private float _rotateValue = 0f;
    
    // Start is called before the first frame update
    private void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        RotateByZ(_rotateValue * _rotateSpeed * Time.deltaTime);
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
}
