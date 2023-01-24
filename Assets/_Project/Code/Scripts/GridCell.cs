using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCell : MonoBehaviour
{
    [SerializeField] private Vector2 _gridPosition;
    
    [SerializeField] private GameObject _bubble;

    [SerializeField] private GameObject[] _connectedBubbles;

    public Vector2 GridPosition
    {
        get => _gridPosition;
        set => _gridPosition = value;
    }
    
    public GameObject Bubble
    {
        get => _bubble;
        set => _bubble = value;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
