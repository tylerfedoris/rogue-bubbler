using System;
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

    [SerializeField] private GameObject _bubblePrefab;

    [SerializeField] private Bubble.BubbleType[] _bubbleTypes; 
    
    [SerializeField] private GameObject _blockerPrefab;

    [SerializeField] private Boundaries _boundaries;
    
    [SerializeField][Range(0f, 1f)] private float _chanceToSpawnBlocker = .25f;

    private GameObject[][] _grid;
    private float _cellSize;
    private GridDimensions _gridDimensions;
    private int _totalCells;
    
    [SerializeField] private int _currentLevel = 1;
    [SerializeField] private int _maxRowGeneration = 8;

    // Start is called before the first frame update
    private void Start()
    {
        GenerateGrid();
    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    private int GetTotalCells(int maxRows, int maxColumns)
    {
        return (maxRows * maxColumns) - (maxRows / 2);
    }

    private void GenerateGrid()
    {
        ClearGrid();

        _cellSize = Bubble.BubbleScale;
        _gridDimensions.MaxRows = 14;
        _gridDimensions.MaxColumns = _gridWidth == GridWidth.Narrow ? 11 : 16;
        _totalCells = GetTotalCells(_gridDimensions.MaxRows, _gridDimensions.MaxColumns);
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
                
                var cellCollider = cell.GetComponent<CircleCollider2D>();
                cellCollider.radius = _cellSize / 2f;
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
        int numBubblesToSpawn = Mathf.Min(UnityEngine.Random.Range(randomSpawnInterval.x, randomSpawnInterval.y), GetTotalCells(_maxRowGeneration, _gridDimensions.MaxColumns));
        int numFirstRowColumns = _grid[0].Length;
        int numBubblesInFirstRow = UnityEngine.Random.Range(2, numFirstRowColumns);

        var cellsToBranchFrom = new List<GridCell>();
        
        var randomColumns = Helpers.GenerateRandomUniqueIndexes(numBubblesInFirstRow, numFirstRowColumns);
        foreach (int column in randomColumns)
        {
            var gridCell = GetGridCell(0, column);
            SpawnBubbleInGridCell(gridCell, ref numBubblesToSpawn);
            cellsToBranchFrom.Add(gridCell);
        }
        
        GenerateBubbles(cellsToBranchFrom, ref numBubblesToSpawn);

        if (numBubblesToSpawn <= 0)
        {
            return;
        }
        
        // if there are still bubbles left to spawn, work backwards to attempt spawning again
        for (int row = _maxRowGeneration; row > 0 && numBubblesToSpawn > 0; row--)
        {
            cellsToBranchFrom = new List<GridCell>();
            for (int column = 0; column < _grid[row].Length; column++)
            {
                var gridCell = GetGridCell(row, column);
                if (gridCell.Bubble)
                {
                    cellsToBranchFrom.Add(GetGridCell(row, column));   
                }
            }
            GenerateBubbles(cellsToBranchFrom, ref numBubblesToSpawn);
        }
        
        if (numBubblesToSpawn <= 0)
        {
            return;
        }

        // if there are STILL bubbles left to spawn, start from the top down this time and include empty cells
        // this might be okay to remove in the future if games get too exhausting at higher levels
        // the chance to have a random easy level as a break might be nice
        for (int row = 0; row < _maxRowGeneration && numBubblesToSpawn > 0; row++)
        {
            cellsToBranchFrom = new List<GridCell>();
            for (int column = 0; column < _grid[row].Length; column++)
            {
                cellsToBranchFrom.Add(GetGridCell(row, column));
            }
            GenerateBubbles(cellsToBranchFrom, ref numBubblesToSpawn);
        }
    }

    private void GenerateBubbles(List<GridCell> cellsToBranchFrom, ref int numBubblesToSpawn)
    {
        while (numBubblesToSpawn > 0 && cellsToBranchFrom.Count > 0)
        {
            var newCellsToBranchFrom = new List<GridCell>();

            foreach (var cell in cellsToBranchFrom)
            {
                var emptyConnectedCells = cell.ConnectedCells.Where(connectedCell => connectedCell.GetComponent<GridCell>().GridPosition.x <= _maxRowGeneration && !connectedCell.GetComponent<GridCell>().Bubble).ToList();
                int numConnectedBubblesToSpawn = UnityEngine.Random.Range(0, emptyConnectedCells.Count);
                var randomCellIndexes = Helpers.GenerateRandomUniqueIndexes(numConnectedBubblesToSpawn, Mathf.Min(emptyConnectedCells.Count, numBubblesToSpawn));
                int numBlockersToSpawn = UnityEngine.Random.Range(0, emptyConnectedCells.Count - numConnectedBubblesToSpawn + 1);
                
                var remainingEmptyCells = Enumerable.Range(0, emptyConnectedCells.Count).Except(randomCellIndexes).ToList();

                foreach (int cellIndex in randomCellIndexes)
                {
                    var gridCell = emptyConnectedCells[cellIndex].GetComponent<GridCell>();
                    SpawnBubbleInGridCell(gridCell, ref numBubblesToSpawn);
                    newCellsToBranchFrom.Add(gridCell);
                }
                
                bool spawnBlockers = UnityEngine.Random.Range(0.0f, 1.0f) > (1f - _chanceToSpawnBlocker);
                
                if (!spawnBlockers)
                {
                    continue;
                }
                
                var randomEmptyCellIndexes = Helpers.GenerateRandomUniqueIndexes(numBlockersToSpawn, remainingEmptyCells.Count);
                
                foreach (int emptyCellIndex in randomEmptyCellIndexes)
                {
                    int cellIndex = remainingEmptyCells[emptyCellIndex];
                    var gridCell = emptyConnectedCells[cellIndex].GetComponent<GridCell>();
                    SpawnBubbleInGridCell(gridCell, ref numBubblesToSpawn, true);
                    newCellsToBranchFrom.Add(gridCell);
                }
            }

            cellsToBranchFrom = newCellsToBranchFrom;
        }
    }

    private void SpawnBubbleInGridCell(GridCell gridCell, ref int numBubblesToSpawn, bool spawnBlocker = false)
    {
        if (gridCell.Bubble)
        {
            Debug.LogErrorFormat("ERROR: {0} already has a bubble", gridCell.gameObject.name);
        }
        
        gridCell.Bubble = Instantiate(_bubblePrefab, gridCell.transform);
        // gridCell.Bubble.GetComponent<Collider2D>().enabled = false;
        var bubbleType = spawnBlocker
            ? Bubble.BubbleType.Blocker
            : _bubbleTypes[UnityEngine.Random.Range(0, _bubbleTypes.Length)];
        var bubble = gridCell.Bubble.GetComponent<Bubble>();
        if (!bubble)
        {
            throw new Exception("No Bubble script was found on the instantiated bubble GameObject.");
        }
        bubble.BubbleTypeProperty = bubbleType;
        numBubblesToSpawn--;
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
                    GetConnectedCells(gridCell, row, column);
                }
            }
        }
    }

    private void GetConnectedCells(GridCell gridCell, int row, int column)
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
            gridCell.AddConnectedCell(GetGridCell(position.x, position.y));
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

    private void ClearGrid()
    {
        if (_grid == null)
        {
            return;
        }
        
        for (var row = 0; row < _grid.Length; row++)
        {
            for (var column = 0; column < _grid[row].Length; column++)
            {
                Destroy(_grid[row][column].gameObject);
            }
        }
    }
}
