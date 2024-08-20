using System;
using System.Collections.Generic;
using System.Linq;
using Grid.Building.Currency;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using Utils;
using Debug = UnityEngine.Debug;

namespace Grid.Building {
    public class GridBuildingSystem : MonoBehaviour {
        [BoxGroup("Debug Grid")]
        [HorizontalGroup("Debug Grid/Split", LabelWidth = 100)] 
        [SerializeField] private bool drawDebugGrid;
    
        [BoxGroup("Debug Grid")]
        [ShowIf("drawDebugGrid")]
        [SerializeField] private Color debugGridColor = Color.red;
    
        [Header("References")]
        [SerializeField] private Tilemap groundTilemap;
        [SerializeField] private PlacedObject buildingParent;
    
    
        [Header("Values")]
        [Tooltip("Cell size divided by Tiles of Building equals perfect fit")]
        [SerializeField, Range(1,5)] private int cellSize = 2;
        [SerializeField] private Vector2 gridOffset;
    
        [Tooltip("Four Corners of the Grid")]
        [SerializeField] private Transform[] boundsTransforms;
        [Tooltip("Building Types")]
        [SerializeField] private List<PlacedObjectTypeSO> buildingObjectTypeSos;
        
        /// <summary>
        /// Return the Base Upgrade of the passed in Upgrade
        /// </summary>
        /// <param name="buildingUpgrade"></param>
        /// <returns></returns>
        public List<BuildingUpgrade> GetAllBuildingUpgrades(BuildingUpgrade buildingUpgrade) {
            foreach (var buildingType in buildingObjectTypeSos) {
                // Check if the buildingType contains the buildingUpgrade
                if (buildingType.upgrades.Contains(buildingUpgrade) || buildingType.BaseBuildingInformation == buildingUpgrade) {
                    // Create a new list with the base upgrade as the first item
                    List<BuildingUpgrade> upgrades = new List<BuildingUpgrade> { buildingType.BaseBuildingInformation };

                    // Add the rest of the upgrades
                    upgrades.AddRange(buildingType.upgrades);

                    return upgrades;
                }
            }

            Debug.LogError("The passed upgrade is not part of any building type.");
            return null;
        }
    
        // Current Building to be placed
        private PlacedObjectTypeSO _placedObjectTypeSo;
        // Index of the currently selected building, which is previewed on that cell but not placed yet
        private int _currentSelectedBuildingGhostIndex;
        // Whether the player is in cell selection mode
        private bool _hasSelectedCell;
        // Cell Coordinates
        private Vector2Int _selectedCellCoordinates;
        // Grid, where each Cell is a GridObject
        private Grid<GridObject> _grid;
        private Camera _mainCamera;
    
        // Weather the Ghost is red or green
        public class CanBuildChangedEventArgs : EventArgs {
            public bool CanBuild { get; }

            public CanBuildChangedEventArgs(bool canBuild) {
                CanBuild = canBuild;
            }
        }
    
        // Player Clicked on a Cell which is occupied by a Building. //Open Upgrade Menu
        public event Action<Vector3> OnUpgradeMenuOpened;
    
        // Upgrade Info Panel or Current Building Info Panel is Opened by Player => Information is getting sent to UI to display
        public event Action<BuildingUpgrade, InformationType> OnBuildingInfoChanged;
    
        // Tells the UI about the Money of the Player to update Text and Color
        public event Action<bool, string> OnMoneyChanged;
    
        // Player clicked on a Cell which is not occupied by a Building and where buildable ground is painted => Open Building Menu
        public event Action<Vector3> OnBuildingMenuOpened;
    
        // Player stopped building (Clicked on the X Button)
        public event Action OnStoppedBuilding; 
    
        // Weather the Ghost is red or green
        public event EventHandler<CanBuildChangedEventArgs> OnCanBuildChanged;
        // Event above only fired when value changed
        private bool _previousCanBuild;
    
        // Despawns the old ghost and spawns a new one with the new position and the new PlacedObjectTypeSO
        public event Action<Vector3, PlacedObjectTypeSO> OnSelectChanged;
    
        private void Awake() {
            _mainCamera = Camera.main;
        
            // Check if there are 4 corners
            if(boundsTransforms.Length != 4) {
                Debug.LogError("Need 4 Corners");
                return;
            }
        
            // Create the Grid
            CreateGrid();

            // Default the first SO to be the default selected one
            _placedObjectTypeSo = buildingObjectTypeSos[0];
        }

        private void Start() {
            // Bind the Money Changed Event
            CurrencyPortfolio.Instance.OnMoneyChanged += UpdateUIIfCanBuild;
        }

        private void OnDestroy() {
            // Unbind the Money Changed Event
            CurrencyPortfolio.Instance.OnMoneyChanged -= UpdateUIIfCanBuild;
        }

        /// <summary>
        /// Create the Grid based on the 4 Corners
        /// </summary>
        private void CreateGrid() {
            // Sort the transforms based on their positions (highest to lowest Y)
            Array.Sort(boundsTransforms, (t1, t2) => t2.position.y.CompareTo(t1.position.y));
        
            // Set the four corners of the grid
            Transform topLeftBound = boundsTransforms[0].position.x < boundsTransforms[1].position.x ? boundsTransforms[0] : boundsTransforms[1];
            Transform topRightBound = boundsTransforms[0].position.x > boundsTransforms[1].position.x ? boundsTransforms[0] : boundsTransforms[1];
            Transform bottomLeftBound = boundsTransforms[2].position.x < boundsTransforms[3].position.x ? boundsTransforms[2] : boundsTransforms[3];
            Transform bottomRightBound = boundsTransforms[2].position.x > boundsTransforms[3].position.x ? boundsTransforms[2] : boundsTransforms[3];

            // Calculate the width and height of the grid
            float gridWidth = Vector3.Distance(topLeftBound.position, topRightBound.position);
            float gridHeight = Vector3.Distance(topLeftBound.position, bottomLeftBound.position);

            // Calculate the center position of the grid
            float centerX = (topLeftBound.position.x + topRightBound.position.x + bottomLeftBound.position.x + bottomRightBound.position.x) / 4;
            float centerY = (topLeftBound.position.y + topRightBound.position.y + bottomLeftBound.position.y + bottomRightBound.position.y) / 4;
            Vector3 centerPosition = new Vector3(centerX, centerY, 0);

            // Calculate how many cells can fit in the grid
            int cellCountWidth = Mathf.FloorToInt(gridWidth / cellSize);
            int cellCountHeight = Mathf.FloorToInt(gridHeight / cellSize);

            // Create the grid
            _grid = new Grid<GridObject>(cellCountWidth, cellCountHeight, cellSize, centerPosition + new Vector3(gridOffset.x, gridOffset.y, 0), ( g,  x,  y) => new GridObject(g, x, y), drawDebugGrid);
        }
        
        
        #region Cell
        public bool SelectCell() {
            if (EventSystem.current.IsPointerOverGameObject()) { // Did we click on the UI?
                return false;
            } 
            
            var mouseWorldPosition = UtilClass.GetMouseWorldPosition(_mainCamera);
            
            // Convert it to XY position on the Grid
            _grid.GetXY(mouseWorldPosition, out var x, out var y);
            
            var gridObject = _grid.GetGridObject(x, y);
            
            // Did we hit a Cell?
            if (gridObject != null) {
                // Is the cell already occupied?
                var placedObject = gridObject.GetPlacedObject();
                if (placedObject != null) { // Building on Cell
                    
                    if (_hasSelectedCell) { // If we have a selected Cell, reset it first
                        ResetSelectedCell();
                        // Despawn the Building Ghost
                        OnStoppedBuilding?.Invoke();
                        // Continue
                    }
                    
                    // Open the Upgrade Menu, because we clicked on a Building
                    OnUpgradeMenuOpened?.Invoke(gridObject.GetCellCenterWorldPosition());
                    
                    //Select the Cell
                    SetSelectedCell(new Vector2Int(x, y));
                    return true;
                }

                // No Building on the Cell, so we can continue
            }
            else { // Clicked outside the Grid
                Debug.Log("Grid not initialized correctly or clicked outside the grid");
                return false;
            }
            
            // Can we fit any of our Building in the List on the selected Cell?
            if (!IsEnoughSpaceForAnyBuildingOfAllBuildings(x, y)) {
                return false;
            }

            if (!_hasSelectedCell) { // If we have no selected cell
                // Select the Cell and continue
                SetSelectedCell(new Vector2Int(x, y));
            } else { // We have a selected cell
                // We are not clicking on the UI and we clicked on another cell
                ResetSelectedCell();
                
                // Set a new selected cell with the x,y of the new clicked on Cell.
                SetSelectedCell(new Vector2Int(x, y));
            }
            
            // Open the Building Menu, because we clicked on a Cell
            OnBuildingMenuOpened?.Invoke(_grid.GetGridObject(x, y).GetCellCenterWorldPosition());
            
            // Spawns a new Building Ghost
            Vector3 worldPosition = _grid.GetWorldPosition(x, y);
            OnSelectChanged?.Invoke(worldPosition, _placedObjectTypeSo);
            
            // The color of the Ghost shall only be representing the build status of the current selected building
            bool canBuild = CanBuildAtCellPosition(x, y, _placedObjectTypeSo);
            
            // Only if we can build, check if we have enough money, in order to update the visual
            if (canBuild) {
                canBuild = CurrencyPortfolio.Instance.CanAfford((int)_placedObjectTypeSo.GetBuildingCost());
            }
            
            // Update the Money Text and Color
            OnMoneyChanged?.Invoke(CurrencyPortfolio.Instance.CanAfford((int)_placedObjectTypeSo.GetBuildingCost()), _placedObjectTypeSo.GetBuildingCost().ToString());
            
            // Update the Color of the Ghost to represent the build status
            OnCanBuildChanged?.Invoke(this, new CanBuildChangedEventArgs(canBuild));

            return true;
        }
        
        /// <summary>
        /// Check if we can build at this grid position
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="buildingToCheck"></param>
        /// <returns></returns>
        private bool CanBuildAtCellPosition(int x, int y, PlacedObjectTypeSO buildingToCheck) {
            // Is the x, y position out of bounds?
            if (!_grid.IsWithinBounds(x, y)) {
                return false;
            }
    
            // Get all occupied grid positions of the selected building
            var gridPositionList = buildingToCheck.GetGridPositionList(new Vector2Int(x, y));
    
            // Check if we can build at each grid positions Center + Corners with offset
            foreach (var gridPosition in gridPositionList) {
            
                var gridObject = _grid.GetGridObject(gridPosition.x, gridPosition.y);
                // If the field is not initialized correctly
                if (gridObject == null) {
                    print("At Least one of the Grid Cells is initialized incorrectly");
                    return false;
                }
    
                var placedObject = gridObject.GetPlacedObject();
    
                // If there is a Building on the Grid Position
                if (placedObject != null) {
                
                    print("At Least one of the Grid Cells is occupied");
                    return false;
                }
    
                // Center of the Cell
                Vector3 worldPosition = _grid.GetWorldPosition(gridPosition.x, gridPosition.y);
                Vector3 tileCenterPosition = new Vector3(worldPosition.x + _grid.GetCellSize() / 2, worldPosition.y + _grid.GetCellSize() / 2, worldPosition.z);
    
                // Define a small offset for the corners
                var cellCornerToCheckIfValidThreshold = 25;
                float offset = _grid.GetCellSize() / cellCornerToCheckIfValidThreshold;
    
                // Define the corners with offset: center, top-left, top-right, bottom-left, bottom-right
                Vector3[] positionsToCheck = {
                    tileCenterPosition,
                    new (tileCenterPosition.x - _grid.GetCellSize() / 2 + offset, tileCenterPosition.y + _grid.GetCellSize() / 2 - offset, tileCenterPosition.z),
                    new (tileCenterPosition.x + _grid.GetCellSize() / 2 - offset, tileCenterPosition.y + _grid.GetCellSize() / 2 - offset, tileCenterPosition.z),
                    new (tileCenterPosition.x - _grid.GetCellSize() / 2 + offset, tileCenterPosition.y - _grid.GetCellSize() / 2 + offset, tileCenterPosition.z),
                    new (tileCenterPosition.x + _grid.GetCellSize() / 2 - offset, tileCenterPosition.y - _grid.GetCellSize() / 2 + offset, tileCenterPosition.z)
                };
    
                // Check if we can build at each corner
                foreach (var position in positionsToCheck) {
                    Vector3Int tilemapPosition = groundTilemap.WorldToCell(position);
                
                    //If there is no Tile-map => (buildable ground) at this position, we cant build
                    if (!CanBuildAtPosition(tilemapPosition)) {
                        return false;
                    }
                }
            }
    
            return true;
        }
        private bool CanBuildAtPosition(Vector3Int gridPosition) {
            return groundTilemap.GetTile(gridPosition) != null;
        }
    
        /// <summary>
        /// Save the values of the selected cell
        /// </summary>
        /// <param name="selectedCellGridCoordinates"></param>
        private void SetSelectedCell(Vector2Int selectedCellGridCoordinates) {
            _hasSelectedCell = true;
            _selectedCellCoordinates = selectedCellGridCoordinates;
        }
        /// <summary>
        /// Unselect the Cell and Hide Building UI
        /// </summary>
        public void ResetSelectedCell() {
            _hasSelectedCell = false;
            _selectedCellCoordinates = Vector2Int.zero;
        
            // Close Building/ Upgrade Menu and hide the Ghost
            OnStoppedBuilding?.Invoke();
        }
    
        #endregion

        #region Building Buildings
        /// <summary>
        /// Information Button on Hover (Ghost, not built yet)
        /// </summary>
        public void SetupBuildingInformation() {
            var buildingGhostBuilding =
                buildingObjectTypeSos[_currentSelectedBuildingGhostIndex].BaseBuildingInformation;

            // Set the Information Type to None if there is no Upgrade, set it to Upgrade if there is one
            InformationType informationType = InformationType.Current;
        
            // Notify the UI about the Upgrade
            OnBuildingInfoChanged?.Invoke(buildingGhostBuilding, informationType);
        }
    
        /// <summary>
        /// Called on Build Button pressed
        /// </summary>
        public void PlaceSelectedBuilding() {
            // Is there buildable ground or is the cell occupied?
            if(!CanBuildAtCellPosition(_selectedCellCoordinates.x, _selectedCellCoordinates.y, _placedObjectTypeSo)) {
                print("Can't build at " + _selectedCellCoordinates.x + ", " + _selectedCellCoordinates.y);
                return;
            }
        
            //Do we have enough money to build? 
            //-> This is seperated, because we want to access SelectionMode even if we dont have enough money
            var buildingCost = _placedObjectTypeSo.GetBuildingCost();
            if(!CurrencyPortfolio.Instance.CanAfford((int)buildingCost)) {
                Debug.Log("Not enough Money");
                return;
            }
        
            // List of Cells that will be Occupied by that building
            var gridPositionList = _placedObjectTypeSo.GetGridPositionList(_selectedCellCoordinates);
        
            // Close the Building Menu, because we built.
            OnStoppedBuilding?.Invoke();
        
            // Instantiate the Building
            CreateBuilding(_selectedCellCoordinates.x, _selectedCellCoordinates.y, gridPositionList);
        
            // Deduct the building cost from the money
            CurrencyPortfolio.Instance.SpendMoney((int)buildingCost);
    
            // Center position of cell to center the Upgrade Menu
            var gridObject = _grid.GetGridObject(_selectedCellCoordinates.x, _selectedCellCoordinates.y);
            Vector3 centerPosition = gridObject.GetCellCenterWorldPosition();
        
            // Open the Upgrade Menu
            OnUpgradeMenuOpened?.Invoke(centerPosition);
        }

        /// <summary>
        /// Instantiates the building, occupy the grid Cells and 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="gridPositionList"></param>
        private void CreateBuilding(int x, int y, List<Vector2Int> gridPositionList) {
            // Instantiate the Parent of the Building
            Transform placedObjectTransform = Instantiate(buildingParent.transform, _grid.GetWorldPosition(x, y), Quaternion.identity);
            placedObjectTransform.name = _placedObjectTypeSo.name;
        
            //Instantiate the Building Visual
            var buildingVisual = Instantiate(_placedObjectTypeSo.BaseBuildingInformation.Prefab, placedObjectTransform.position, Quaternion.identity);
            buildingVisual.SetParent(placedObjectTransform);
        
            PlacedObject placedObject = placedObjectTransform.GetComponent<PlacedObject>();
            placedObject.Init(new Vector2Int(x, y), _placedObjectTypeSo);
                
            // Occupy each affected Grid cell with this Building
            foreach (var gridPosition in gridPositionList) {
                _grid.GetGridObject(gridPosition.x, gridPosition.y).SetPlacedObject(placedObject);
            }
            
            var occupiedPositionsByBuilding = placedObject.GetGridPositionList();
            _grid.TriggerGridObjectChanged(placedObject, occupiedPositionsByBuilding);
        
            //Because we are on a Grid, we need to set the parent to the Grid to render it correctly
            placedObject.transform.SetParent(transform);
        }

        /// <summary>
        /// Destroys the Building on the selected cell if there is one + unoccupy the affected grid cells
        /// Method is called from the Upgrade Menu, so we have to have a building selected, otherwise this should not be called
        /// </summary>
        public void DeleteSelectedBuildingOnGrid() {
            // Grid Object on the selected Cell
            var gridObject = _grid.GetGridObject(_selectedCellCoordinates.x, _selectedCellCoordinates.y);
            // If the field is not initialized correctly
            if (gridObject == null) {
                Debug.Log("No Building to delete on " + _selectedCellCoordinates.x + ", " + _selectedCellCoordinates.y);
                return;
            }

            var placedObject = gridObject.GetPlacedObject();

            // If there is a Building on the Grid Position, Should never be null, because we are already in the Upgrade menu
            if (placedObject != null) {
                // Destroy the Building
                placedObject.DestroySelf();
        
                //Reward the Player
                CurrencyPortfolio.Instance.EarnMoney((int)placedObject.PlacedBuildingInformation.Cost / 2);
            
                Debug.Log("Sold Building for " + (int)placedObject.PlacedBuildingInformation.Cost / 2);
            
                var gridPositionList = placedObject.GetGridPositionList();
                _grid.TriggerGridObjectChanged(null, gridPositionList);
                
                //Cycle through all its occupied gridPositions and clear them
                foreach (var gridPosition in gridPositionList) {
                    var gridObjectToClear = _grid.GetGridObject(gridPosition.x, gridPosition.y);
                    if (gridObjectToClear != null) {
                        gridObjectToClear.ClearPlacedObject();
                    }
                }
            }
            else {
                Debug.LogError("No Building to delete, this should not happen");
            }
        }

        /// <summary>
        /// Called from increment/ decrement button, changes the selected Building and updating Building Ghost visual
        /// </summary>
        /// <param name="increment"></param>
        public void ChangeSelectedBuilding(bool increment) {
            // Increment if true, decrement if false
            _currentSelectedBuildingGhostIndex = increment ? _currentSelectedBuildingGhostIndex + 1 : _currentSelectedBuildingGhostIndex - 1;

            // If currentTypeIndex is out of range, reset it to 0
            if(_currentSelectedBuildingGhostIndex >= buildingObjectTypeSos.Count) {
                _currentSelectedBuildingGhostIndex = 0;
            }
            else if(_currentSelectedBuildingGhostIndex < 0) { // Reset to max
                _currentSelectedBuildingGhostIndex = buildingObjectTypeSos.Count - 1;
            }
        
            // Set the current placedObjectTypeSo to the one at currentTypeIndex
            _placedObjectTypeSo = buildingObjectTypeSos[_currentSelectedBuildingGhostIndex];
        
            var currentSelectedCellWorldPosition = _grid.GetWorldPosition(_selectedCellCoordinates.x, _selectedCellCoordinates.y);
        
            // Change the selected building ghost visual
            OnSelectChanged?.Invoke(currentSelectedCellWorldPosition, _placedObjectTypeSo);
        
            // Can we build at all the cells this building would occupy?
            bool canBuild = CanBuildAtCellPosition(_selectedCellCoordinates.x, _selectedCellCoordinates.y, _placedObjectTypeSo);
            var canAfford = CurrencyPortfolio.Instance.CanAfford((int)_placedObjectTypeSo.GetBuildingCost());

            //If we can build, check if we have enough money, in order to update the visual
            if (canBuild) {
                canBuild = canAfford;
            }
        
            // Update Money amount and Text Color
            OnMoneyChanged?.Invoke(canAfford, _placedObjectTypeSo.GetBuildingCost().ToString());

            // Update the Color of the Ghost to represent the build status
            OnCanBuildChanged?.Invoke(this, new CanBuildChangedEventArgs(canBuild));
        }

        /// <summary>
        /// Go through all Buildings in List and checks if any would fit on the selected Cell and neighboring Cells
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private bool IsEnoughSpaceForAnyBuildingOfAllBuildings(int x, int y) {
            foreach (var buildingObject in buildingObjectTypeSos) {
            
                if (CanBuildAtCellPosition(x, y, buildingObject)) {
                    return true;
                }
            }
            return false;
        } 
    
        #endregion

        #region Upgrading Building
    
        /// <summary>
        /// Opens the Upgrade Information Panel and Notifies the UI about the next Upgrade
        /// </summary>
        public void OpenUpgradeInformation() {
            // Get the next Upgrade of the Building
            var nextUpgrade = GetBuildingUpgrade(false);

            // Set the Information Type to None if there is no Upgrade, set it to Upgrade if there is one
            InformationType informationType = nextUpgrade switch {
                // Nulling means the information in text will be nulled
                null => InformationType.None,
                // If we have an Upgrade, to tell we take the information from the Upgrade provided
                { } upgrade => InformationType.Upgrade
            };
        
            // Notify the UI about the Upgrade
            OnBuildingInfoChanged?.Invoke(nextUpgrade, informationType);
        }

        /// <summary>
        /// Opens the Current Building Information Panel and Notifies the UI about the current Building
        /// </summary>
        public void OpenCurrentBuildingInformation() {
            // Get the next Upgrade of the Building
            var baseBuildingInformation = GetBuildingUpgrade(true);
        
            // Tell the UI about the Current Building and set the Information Type to Current, to know we set the Information values to the ones provided
            OnBuildingInfoChanged?.Invoke(baseBuildingInformation, InformationType.Current);
        }
    
        /// <summary>
        /// Get the next Upgrade of the Building
        /// </summary>
        /// <returns></returns>
        private BuildingUpgrade GetBuildingUpgrade(bool currentBuildingUpgrade) {
            var gridObject = _grid.GetGridObject(_selectedCellCoordinates.x, _selectedCellCoordinates.y);
        
            if(gridObject == null) {
                Debug.LogError("No GridObject found at: " + _selectedCellCoordinates.x + ", " + _selectedCellCoordinates.y);
                return null;
            }

            var placedObject = gridObject.GetPlacedObject();
        
            if(placedObject == null) {
                Debug.LogError("No Building found at " + _selectedCellCoordinates.x + ", " + _selectedCellCoordinates.y);
                return null;
            }
        
            var buildingUpgrade = currentBuildingUpgrade switch {
                // Get the current Information of the Building
                true => gridObject.GetPlacedObject().PlacedBuildingInformation,
                // Get the Information of the next Upgrade
                false => gridObject.GetPlacedObject().GetNextUpgrade()
            };

            return buildingUpgrade;
        }
        /// <summary>
        /// Called from the Upgrade Menu Upgrade Button, Tries to upgrade the Building
        /// </summary>
        public void RequestUpgradeBuilding() {
            var gridObject = _grid.GetGridObject(_selectedCellCoordinates.x, _selectedCellCoordinates.y);
        
            // Upgrade the Building
            gridObject.UpgradeBuilding();
        }
    
        #endregion
        
        /// <summary>
        /// Updates the UI, based if we can afford the selected building
        /// </summary>
        private void UpdateUIIfCanBuild(int newMoneyAmount)
        {
            //If we dont have a selected cell, we dont need to update the CanBuild Status
            if(!_hasSelectedCell) {
                return;
            }
        
            // Can we build at the selected cell?
            bool canBuild = CanBuildAtCellPosition(_selectedCellCoordinates.x, _selectedCellCoordinates.y, _placedObjectTypeSo);
            var canAfford = _placedObjectTypeSo.GetBuildingCost() <= newMoneyAmount;
        
            // If we can build, override it with the canAfford value
            if (canBuild) {
                canBuild = canAfford;
            }
        
            // Update the Money Text and Color
            OnMoneyChanged?.Invoke(canAfford, _placedObjectTypeSo.GetBuildingCost().ToString());
            // Update the Color of the Ghost
            OnCanBuildChanged?.Invoke(this, new CanBuildChangedEventArgs(canBuild));
        }
        
        #region Gizmos

        private void OnDrawGizmos() {
            if (!drawDebugGrid) {
                return;
            }
            //Dont draw the debug grid in Playmode
            if (Application.isPlaying) {
                return;
            }
        
            if (boundsTransforms == null || boundsTransforms.Length != 4) {
                return;
            }

            // Sort the transforms based on their positions
            Array.Sort(boundsTransforms, (t1, t2) => (t2.position.y.CompareTo(t1.position.y)));

            // The top left and top right transforms
            Transform topLeftBound = boundsTransforms[0].position.x < boundsTransforms[1].position.x ? boundsTransforms[0] : boundsTransforms[1];
            Transform topRightBound = boundsTransforms[0].position.x > boundsTransforms[1].position.x ? boundsTransforms[0] : boundsTransforms[1];

            // The bottom left and bottom right transforms
            Transform bottomLeftBound = boundsTransforms[2].position.x < boundsTransforms[3].position.x ? boundsTransforms[2] : boundsTransforms[3];
            Transform bottomRightBound = boundsTransforms[2].position.x > boundsTransforms[3].position.x ? boundsTransforms[2] : boundsTransforms[3];

            // Calculate the width and height of the grid
            float gridWidth = Vector3.Distance(topLeftBound.position, topRightBound.position);
            float gridHeight = Vector3.Distance(topLeftBound.position, bottomLeftBound.position);

            // Convert the width and height to the number of cells
            int cellCountWidth = Mathf.FloorToInt(gridWidth / cellSize);
            int cellCountHeight = Mathf.FloorToInt(gridHeight / cellSize);

            // Draw the grid
            Gizmos.color = debugGridColor;
            for (int x = 0; x < cellCountWidth; x++) {
                for (int y = 0; y < cellCountHeight; y++) {
                    Vector3 centerPosition = new Vector3(topLeftBound.position.x + (x * cellSize) + cellSize / 2, topLeftBound.position.y - (y * cellSize) - cellSize / 2, 0);
                    Gizmos.DrawWireCube(centerPosition + new Vector3(gridOffset.x, gridOffset.y, 0), new Vector3(cellSize, cellSize, 0));
                }
            }
        }

        [Button("Print All Placed Building Names")]
        private void PrintAllPlacedBuildingNames() {
            var buildingNames = 
                _grid.GetCorrectBuildingDataOfAllObjectsPlacedOnGrid().
                    Select(obj => obj.Name).
                    ToList();

            ColorfulDebug.LogWithRandomColor(buildingNames);
        }
        #endregion

        public Grid<GridObject> GetGrid() {
            return _grid;
        }
    }
}