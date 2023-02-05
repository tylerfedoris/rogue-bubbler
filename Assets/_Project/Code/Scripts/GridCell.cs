using System;
using System.Collections.Generic;
using UnityEngine;

public class GridCell : MonoBehaviour
{
    [SerializeField] private Vector2Int _gridPosition;
    
    [SerializeField] private GameObject _bubble;

    [SerializeField] private List<GridCell> _connectedCells = new();

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
    
    public List<GridCell> ConnectedCells
    {
        get => _connectedCells;
        set => _connectedCells = value;
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void AddConnectedCell(GridCell cell)
    {
        _connectedCells.Add(cell);
    }

    public void ClearConnectedCells()
    {
        _connectedCells.Clear();
    }
}
