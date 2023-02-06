using System.Collections.Generic;
using UnityEngine;

namespace _Project.Code.Scripts
{
    public class GridCell : MonoBehaviour
    {
        [SerializeField] private Vector2Int _gridPosition;
    
        [SerializeField] private GameObject _bubble;

        [SerializeField] private List<GridCell> _connectedCells = new();
        
        public bool PendingBubbleDelete { get; set; }
        public bool TaggedInSearch { get; set; }

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

        public void AddConnectedCell(GridCell cell)
        {
            _connectedCells.Add(cell);
        }

        public void ClearConnectedCells()
        {
            _connectedCells.Clear();
        }
    }
}
