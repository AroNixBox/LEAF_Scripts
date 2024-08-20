using System;
using System.Collections.Generic;
using System.Linq;
using Grid.Building;
using TMPro;
using UnityEngine;

namespace Grid {
    public class Grid<TGridObject> : IDisposable {
        // Invoked when building is placed, upgraded or deleted
        public event EventHandler<GridValueChangedEventArgs<PlacedObject>> OnPlacedObjectChanged;
        
        // Stores all Buildings placed on the Grid, works like Einwohnermeldeamt
        private readonly Dictionary<Vector2Int, BuildingUpgrade> _currentUpgradesOfBuildings = new ();
        public class GridValueChangedEventArgs<T> : EventArgs {
            public T Value;
            public List<Vector2Int> CellCoordinates;
        }
    
        // Entire Grid width
        private readonly int _width;
        // Entire Grid height
        private readonly int _height;
        // Size of each cell
        private readonly float _cellSize;
        // Middle of the Grid
        private readonly Vector3 _originPosition;
    
        // 2D Array of the Grid
        private readonly TGridObject[,] _gridArray;

        // Should we spawn TMP Texts for Debugging?

        public Grid(int width, int height, float cellSize, Vector3 centerPosition, Func<Grid<TGridObject>, int, int, TGridObject> createGridObject, bool showDebug)
        {
            _width = width;
            _height = height;
            _cellSize = cellSize;

            //Calculate the origin position
            _originPosition = centerPosition - new Vector3(width, height) * cellSize * 0.5f;
        
            _gridArray = new TGridObject[width, height];
        
            // 2D Array of the Debug Text
            var debugTextArray = new TextMeshPro[width, height];

            //Create the Grid Object with whatever type is passed in
            for(int x = 0; x < _gridArray.GetLength(0); x++)
            {
                for(int y = 0; y < _gridArray.GetLength(1); y++)
                {
                    _gridArray[x, y] = createGridObject(this, x, y);
                }
            }

            // Subscribe to the event to track which buildings are placed on the grid
            OnPlacedObjectChanged += RefreshPlacedObjectList;
        
            if(!showDebug) { return; }
        
            var parent = new GameObject("Visual Grid").transform;
        
            //Cycle through the first dimension of the array
            for(int x = 0; x < _gridArray.GetLength(0); x++)
            {
                //Cycle through the second dimension of the array
                for(int y = 0; y < _gridArray.GetLength(1); y++)
                {
                    debugTextArray[x, y] = UtilClass.CreateWorldText(
                        _gridArray[x, y]?.ToString(), parent, 
                        //Center the text in the middle of the cell
                        GetWorldPosition(x, y) + new Vector3(cellSize, cellSize) * 0.5f, 
                        cellSize, 3, Color.white, TextAlignmentOptions.Center, 10);
                
                
                    //Vertical lines
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y +1), Color.white, Mathf.Infinity);
                
                    //Horizontal lines
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x +1, y), Color.white, Mathf.Infinity);
                }
            }
            //Horizontal outside line
            Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, Mathf.Infinity);
        
            //Vertical outside line
            Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, Mathf.Infinity);
        
            // Subscribe to the OnPlacedObjectChanged event
            OnPlacedObjectChanged += (_, eventArgs) => {
                foreach (var cellCoordinate in eventArgs.CellCoordinates) {
                    debugTextArray[cellCoordinate.x, cellCoordinate.y].text = GetDebugText(eventArgs.Value, _gridArray[cellCoordinate.x, cellCoordinate.y]);
                }
            };
        }
        /// <summary>
        /// Print the Coordinates and Name of current Building Upgrade if there is one on the Grid
        /// </summary>
        /// <param name="placedObject"></param>
        /// <param name="gridObject"></param>
        /// <returns></returns>
        private string GetDebugText(PlacedObject placedObject, TGridObject gridObject) {
            // No Upgrade => Print Coordinates only
            if (placedObject == null) {
                return gridObject.ToString();
            }

            // Print the current Building Upgrade Name
            var buildingInfo = placedObject.IsInBaseState() 
                ? placedObject.GetData().BaseBuildingInformation.Name 
                : placedObject.GetCurrentUpgrade().Name;

             // .ToString() Method is overridden in PlacedObject.cs
            return $"{gridObject}\n{buildingInfo}";
        }

        /// <summary>
        /// The center XY of a building that occupies more than one Cell
        /// </summary>
        /// <param name="occupiedCells"></param>
        /// <returns></returns>
        private Vector2Int CalculateCenter(List<Vector2Int> occupiedCells) { 
            int totalX = 0;
            int totalY = 0;
            foreach (var cell in occupiedCells) {
                totalX += cell.x;
                totalY += cell.y;
            }
            return new Vector2Int(totalX / occupiedCells.Count, totalY / occupiedCells.Count);
        }
        /// <summary>
        /// Refresh the Data of the Grid on a specific Cell
        /// The reason this is here and not in GridBuildingSystem is that I want to print the data of the building on the grid anyways.
        /// </summary>
        /// <param name="placedBuilding"></param>
        /// <param name="cellCoordinates"></param>
        public void TriggerGridObjectChanged(PlacedObject placedBuilding, List<Vector2Int> cellCoordinates)
        {
            OnPlacedObjectChanged?.Invoke(this, new GridValueChangedEventArgs<PlacedObject> {
                Value = placedBuilding,
                CellCoordinates = cellCoordinates
            });
        }
        
        /// <summary>
        /// Converts XY position to World Position
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Vector3 GetWorldPosition(int x, int y) {
            return new Vector3(x, y) * _cellSize + _originPosition;
        }
    
        /// <summary>
        /// Converts world position to a XY position
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void GetXY(Vector3 worldPosition, out int x, out int y) {
            //World position of 0.5 will be on Grid position 0
            //World position of 1.5 will be on Grid position 1
            x = Mathf.FloorToInt((worldPosition - _originPosition).x / _cellSize);
            y = Mathf.FloorToInt((worldPosition- _originPosition).y / _cellSize);  
        }
    
        /// <summary>
        /// Get the Value based on the Grid Position
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public TGridObject GetGridObject(int x, int y) {
            return IsWithinBounds(x, y) ? _gridArray[x, y] :
                //Value is Invalid, but throw no exception
                default(TGridObject);
        }
    
        //Get the value based on the world position
        public TGridObject GetGridObject(Vector3 worldPosition) {
            GetXY(worldPosition, out var x, out var y);
        
            return GetGridObject(x, y);
        }
    
        /// <summary>
        /// Are we within the bounds of the grid?
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool IsWithinBounds(int x, int y) {
            return x >= 0 && y >= 0 && x < _width && y < _height;
        }

        /// <summary>
        /// Cell Size of the Cell
        /// </summary>
        /// <returns></returns>
        public float GetCellSize() {
            return _cellSize;
        }

        #region Building Upgrade Tracking

        /// <summary>
        /// Return a List of each BuildingUpgrade of the building placed on the grid
        /// </summary>
        /// <returns></returns>
        // TODO: If we want to know the coordinated of the buildings, return the Dictionary instead of the List
        public List<BuildingUpgrade> GetCorrectBuildingDataOfAllObjectsPlacedOnGrid() => new (_currentUpgradesOfBuildings.Values);
        
        /// <summary>
        /// Refresh the List of Placed Objects
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="cellDetails"></param>
        private void RefreshPlacedObjectList(object sender, GridValueChangedEventArgs<PlacedObject> cellDetails) {
            // Extract the placed object
            var building = cellDetails.Value;
            
            // Determine the center coordinates based on the cell details
            var centerCoordinates = cellDetails.CellCoordinates.Count > 1 
                ? CalculateCenter(cellDetails.CellCoordinates) 
                : cellDetails.CellCoordinates[0];

            if (building == null) {
                // If the building is null, remove it from the current upgrades
                _currentUpgradesOfBuildings.Remove(centerCoordinates);

            } else {
                // If the building is not null, determine the building information based on its state
                var buildingInfo = building.IsInBaseState() 
                    ? building.GetData().BaseBuildingInformation 
                    : building.GetCurrentUpgrade();

                // Add or update the building information in the current upgrades
                _currentUpgradesOfBuildings[centerCoordinates] = buildingInfo;
            }
        }
        #endregion

        public void Dispose() { // Unsubscribe from the event
            OnPlacedObjectChanged -= RefreshPlacedObjectList;
        }
    }
}
