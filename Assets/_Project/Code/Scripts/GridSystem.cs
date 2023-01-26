using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSystem : MonoBehaviour
{
    private struct GridDimensions
    {
        public int MaxRows;
        public int MaxColumns;
    }
    
    [Serializable]
    private struct Boundaries
    {
        public GameObject LeftBoundary;
        public GameObject RightBoundary;
        public GameObject TopBoundary;
        public GameObject BottomBoundary;
    }

    [Serializable]
    private enum GridWidth
    {
        Wide,
        Narrow
    }
    
    [SerializeField] private GridWidth _gridWidth = GridWidth.Narrow;
    
    [SerializeField] private GameObject _cellPrefab;

    [SerializeField] private GameObject[] _bubblePrefabs;
    
    [SerializeField] private GameObject _blockerPrefab;

    [SerializeField] private Boundaries _boundaries;

    private GameObject[][] _grid;
    private float _cellSize;
    private GridDimensions _gridDimensions;

    // Start is called before the first frame update
    private void Start()
    {
        InitializeGrid();
    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    private void InitializeGrid()
    {
        _cellSize = _cellPrefab.transform.localScale.x;
        _gridDimensions.MaxRows = 14;
        _gridDimensions.MaxColumns = _gridWidth == GridWidth.Narrow ? 11 : 16;
        _grid = new GameObject[_gridDimensions.MaxRows][];

        SetBoundaryPositionAndScale();
        
        for (var row = 0; row < _gridDimensions.MaxRows; row++)
        {
            bool isRowOdd = row % 2 != 0;
            int columnCount = isRowOdd ? _gridDimensions.MaxColumns - 1 : _gridDimensions.MaxColumns;
            _grid[row] = new GameObject[columnCount];
            for (var column = 0; column < columnCount; column++)
            {
                var cell = Instantiate(_cellPrefab, transform);
                
                cell.transform.localPosition =
                    new Vector3(column * _cellSize + (Convert.ToInt16(isRowOdd) * (_cellSize / 2f)), row * (-_cellSize + (_cellSize / 10f)),
                        0f);
                
                cell.name = "Cell[" + row + ", " + column + "]";
                
                _grid[row][column] = cell;
                
                var gridCell = cell.GetComponent<GridCell>();
                gridCell.GridPosition = new Vector2Int(row, column);
                
                var randomIndex = UnityEngine.Random.Range(0, _bubblePrefabs.Length);
                gridCell.Bubble = Instantiate(_bubblePrefabs[randomIndex], cell.transform);
            }
        }

        LinkConnectedBubbles();
    }

    private void SetBoundaryPositionAndScale()
    {
        var leftBoundaryTransform = _boundaries.LeftBoundary.transform;
        var leftBoundaryPosition = leftBoundaryTransform.position;
        leftBoundaryTransform.position = new Vector3(_gridWidth == GridWidth.Narrow ? -2.9f : -4.15f, leftBoundaryPosition.y, leftBoundaryPosition.z);
        
        var rightBoundaryTransform = _boundaries.RightBoundary.transform;
        var rightBoundaryPosition = rightBoundaryTransform.position;
        rightBoundaryTransform.position = new Vector3(_gridWidth == GridWidth.Narrow ? 2.9f : 4.15f, rightBoundaryPosition.y, rightBoundaryPosition.z);

        var gridTransform = transform;
        var gridPosition = gridTransform.position;
        gridTransform.position = new Vector3(leftBoundaryTransform.position.x + 0.4f, gridPosition.y, gridPosition.z);
        
        var topBoundaryTransform = _boundaries.TopBoundary.transform;
        var topBoundaryScale = topBoundaryTransform.localScale;
        topBoundaryTransform.localScale = new Vector3(topBoundaryScale.x,_gridWidth == GridWidth.Narrow ? 6f : 8.5f, topBoundaryScale.z);
        
        var bottomBoundaryTransform = _boundaries.BottomBoundary.transform;
        var bottomBoundaryScale = bottomBoundaryTransform.localScale;
        bottomBoundaryTransform.localScale = new Vector3(bottomBoundaryScale.x,_gridWidth == GridWidth.Narrow ? 6f : 8.5f, bottomBoundaryScale.z);
    }

    private void LinkConnectedBubbles()
    {
        for (var row = 0; row < _grid.Length; row++)
        {
            for (var column = 0; column < _grid[row].Length; column++)
            {
                var gridCell = _grid[row][column].GetComponent<GridCell>();
                if (gridCell)
                {
                    GetConnectedBubbles(ref gridCell, row, column);
                }
            }
        }
    }

    private void GetConnectedBubbles(ref GridCell gridCell, int row, int column)
    {
        if (!gridCell.Bubble)
        {
            return;
        }
        
        gridCell.ClearConnectedBubbles();

        bool isRowEven = row % 2 == 0;
        var positionsToCheck = new List<Vector2Int>
        {
            new(row - 1, column),
            new(row + 1, column),
            new(row, column - 1),
            new(row, column + 1),
            new(row - 1, isRowEven ? column - 1 : column + 1),
            new(row + 1, isRowEven ? column - 1 : column + 1),
        };

        foreach (var position in positionsToCheck)
        {
            if (!IsValidGridPosition(position.x, position.y))
            {
                continue;
            }
            
            var bubble = _grid[position.x][position.y].GetComponent<GridCell>().Bubble;
            if (bubble)
            {
                gridCell.AddConnectedBubble(bubble);
            }
        }
    }

    private bool IsValidGridPosition(int row, int column)
    {
        return row >= 0 && row < _grid.Length && column >= 0 && column < _grid[row].Length;
    }
}
