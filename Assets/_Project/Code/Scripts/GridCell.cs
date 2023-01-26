using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCell : MonoBehaviour
{
    public static event Action<Vector2Int> OnBubblePlaced;
    
    [SerializeField] private Vector2Int _gridPosition;
    
    [SerializeField] private GameObject _bubble;

    [SerializeField] private List<GameObject> _connectedBubbles = new();

    public Vector2Int GridPosition
    {
        get => _gridPosition;
        set => _gridPosition = value;
    }
    
    public GameObject Bubble
    {
        get => _bubble;
        set => _bubble = value;
    }
    
    public List<GameObject> ConnectedBubbles
    {
        get => _connectedBubbles;
        set => _connectedBubbles = value;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AddConnectedBubble(GameObject bubble)
    {
        _connectedBubbles.Add(bubble);
    }

    public void ClearConnectedBubbles()
    {
        _connectedBubbles.Clear();
    }
}
