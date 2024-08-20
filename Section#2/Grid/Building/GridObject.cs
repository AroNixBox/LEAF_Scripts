using Grid.Building.Currency;
using UnityEngine;

namespace Grid.Building {
    /// <summary>
        /// When the grid is created, each cell is initialized with a GridObject
        /// </summary>
        public class GridObject {
            private readonly Grid<GridObject> _grid;
            private readonly int _x;
            private readonly int _y;
            private PlacedObject _placedObject;
        
            /// <summary>
            /// locally store this x, y and the grid reference
            /// </summary>
            /// <param name="grid"></param>
            /// <param name="x"></param>
            /// <param name="y"></param>
            public GridObject(Grid<GridObject> grid, int x, int y) {
                _grid = grid;
                _x = x;
                _y = y;
            }
            
            /// <summary>
            /// Assign a building to this cell
            /// </summary>
            /// <param name="placedObject"></param>
            public void SetPlacedObject(PlacedObject placedObject) {
                _placedObject = placedObject;
                // Notify the Grid, needs to be after the Upgrade itself
            }
            
            public void UpgradeBuilding() {
                // Any Upgrade?
                if (_placedObject.GetNextUpgrade() == null) {
                    Debug.Log("No more upgrades available for this building.");
                    return;
                }
                
                var data = _placedObject.GetData();

                if (!data) {
                    Debug.LogError("No Data found on PlacedObject");
                    return;
                }
        
                // Can we Afford the Upgrade?
                if (!CurrencyPortfolio.Instance.CanAfford((int)_placedObject.GetNextUpgrade().Cost)) {
                    Debug.Log("Not enough money to upgrade.");
                    return;
                }
        
                // Remove Money
                CurrencyPortfolio.Instance.SpendMoney((int)_placedObject.GetNextUpgrade().Cost);
                // Upgrade the Building
                _placedObject.Upgrade();
                
                // Notify the Grid, needs to be after the Upgrade itself
                var occupiedPositionsByBuilding = _placedObject.GetGridPositionList();
                _grid.TriggerGridObjectChanged(_placedObject, occupiedPositionsByBuilding);
            }
        
            /// <summary>
            /// Center of the Cell
            /// </summary>
            /// <returns></returns>
            public Vector3 GetCellCenterWorldPosition() {
                Vector3 worldPosition = _grid.GetWorldPosition(_x, _y);
                Vector3 tileCenterPosition = new Vector3(worldPosition.x + _grid.GetCellSize() / 2, worldPosition.y + _grid.GetCellSize() / 2, worldPosition.z);
                return tileCenterPosition;
            }
            /// <summary>
            /// Returns the Building on this Cell
            /// </summary>
            /// <returns></returns>
            public PlacedObject GetPlacedObject() {
                return _placedObject;
            }
        
            /// <summary>
            /// Clear the Building on this Cell
            /// </summary>
            public void ClearPlacedObject() {
                _placedObject = null;
            }
            /// <summary>
            /// Prints the x, y of the cell on the World spaced TextMeshPro
            /// </summary>
            /// <returns></returns>
            public override string ToString() {
                return _x + ", " + _y + "\n";
            }
        }
}