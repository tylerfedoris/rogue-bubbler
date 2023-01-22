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
    
    [SerializeField]
    private GridWidth _gridWidth = GridWidth.Narrow;
    
    [SerializeField]
    private GameObject _cellPrefab;

    [SerializeField]
    private Boundaries _boundaries;

    private GameObject[][] _grid;
    private float _cellSize;
    private GridDimensions _gridDimensions;

    private void Awake()
    {
        SetBoundaryPositionAndScale();
    }

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
            }
        }
    }

    private void SetBoundaryPositionAndScale()
    {
        var leftBoundaryPosition = _boundaries.LeftBoundary.transform.position;
        _boundaries.LeftBoundary.transform.position = new Vector3(_gridWidth == GridWidth.Narrow ? -2.9f : -4.15f, leftBoundaryPosition.y, leftBoundaryPosition.z);
        
        var rightBoundaryPosition = _boundaries.RightBoundary.transform.position;
        _boundaries.RightBoundary.transform.position = new Vector3(_gridWidth == GridWidth.Narrow ? 2.9f : 4.15f, rightBoundaryPosition.y, rightBoundaryPosition.z);

        var gridPosition = transform.position;
        transform.position = new Vector3(_boundaries.LeftBoundary.transform.position.x + 0.4f, gridPosition.y, gridPosition.z);
        
        var topBoundaryScale = _boundaries.TopBoundary.transform.localScale;
        _boundaries.TopBoundary.transform.localScale = new Vector3(topBoundaryScale.x,_gridWidth == GridWidth.Narrow ? 6f : 8.5f, topBoundaryScale.z);
        
        var bottomBoundaryScale = _boundaries.BottomBoundary.transform.localScale;
        _boundaries.BottomBoundary.transform.localScale = new Vector3(bottomBoundaryScale.x,_gridWidth == GridWidth.Narrow ? 6f : 8.5f, bottomBoundaryScale.z);
    }
}
