using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private int _totalCells;
    
    [SerializeField] private int _currentLevel = 1;

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
        _totalCells = (_gridDimensions.MaxRows * _gridDimensions.MaxColumns) - (_gridDimensions.MaxRows / 2);
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
            }
        }

        LinkConnectedCells();

        GenerateBubbles();
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

    private void GenerateBubbles()
    {
        var randomSpawnInterval = GetSpawnInterval();
        int numBubblesToSpawn = UnityEngine.Random.Range(randomSpawnInterval.x, randomSpawnInterval.y);
        int numFirstRowColumns = _grid[0].Length;
        int numBubblesInFirstRow = UnityEngine.Random.Range(numFirstRowColumns / 3, numFirstRowColumns);
        
        Debug.LogFormat("randomSpawnInterval: {0}, numBubblesToSpawn: {1}, numBubblesInFirstRow: {2}, numFirstRowColumns: {3}", 
            randomSpawnInterval, numBubblesToSpawn, numBubblesInFirstRow, numFirstRowColumns);
        
        var columnsToPlaceBubbles = Helpers.GenerateRandomUniqueNumberList(numBubblesInFirstRow, 0, numFirstRowColumns);
        foreach (int column in columnsToPlaceBubbles)
        {
            Debug.LogFormat("placed in column: {0}", column);
            var gridCell = GetGridCell(0, column);
            var bubbleToSpawn = _bubblePrefabs[UnityEngine.Random.Range(0, _bubblePrefabs.Length)];
            gridCell.Bubble = Instantiate(bubbleToSpawn, gridCell.transform);
        }
    }

    private void LinkConnectedCells()
    {
        for (var row = 0; row < _grid.Length; row++)
        {
            for (var column = 0; column < _grid[row].Length; column++)
            {
                var gridCell = GetGridCell(row, column);
                if (gridCell)
                {
                    GetConnectedCells(ref gridCell, row, column);
                }
            }
        }
    }

    private void GetConnectedCells(ref GridCell gridCell, int row, int column)
    {
        if (gridCell.ConnectedCells.Count > 0)
        {
            gridCell.ClearConnectedCells();   
        }

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

        foreach (var position in positionsToCheck.Where(position => IsValidGridPosition(position.x, position.y)))
        {
            gridCell.AddConnectedCell(_grid[position.x][position.y]);
        }
    }

    private bool IsValidGridPosition(int row, int column)
    {
        return row >= 0 && row < _grid.Length && column >= 0 && column < _grid[row].Length;
    }

    private Vector2Int GetSpawnInterval()
    {
        return _currentLevel switch
        {
            >= 1 and <= 5 => new Vector2Int(Mathf.CeilToInt(_totalCells / 8f), Mathf.CeilToInt(_totalCells / 4f) + 1),
            >= 6 and <= 10 => new Vector2Int(Mathf.CeilToInt(_totalCells / 6f), Mathf.CeilToInt(_totalCells / 4f) + 1),
            >= 11 and <= 15 => new Vector2Int(Mathf.CeilToInt(_totalCells / 5f), Mathf.CeilToInt(_totalCells / 3f) + 1),
            >= 16 and <= 20 => new Vector2Int(Mathf.CeilToInt(_totalCells / 4f), Mathf.CeilToInt(_totalCells / 3f) + 1),
            >= 21 and <= 25 => new Vector2Int(Mathf.CeilToInt(_totalCells / 4f), Mathf.CeilToInt(_totalCells / 2f) + 1),
            >= 26 and <= 30 => new Vector2Int(Mathf.CeilToInt(_totalCells / 3f), Mathf.CeilToInt(_totalCells / 2f) + 1),
            >= 31 => new Vector2Int(Mathf.CeilToInt(_totalCells / 2f), Mathf.CeilToInt(_totalCells / 2f) + 1),
            _ => new Vector2Int(0, 0)
        };
    }

    private GridCell GetGridCell(int row, int column)
    {
        return _grid[row][column].GetComponent<GridCell>();
    }
}
