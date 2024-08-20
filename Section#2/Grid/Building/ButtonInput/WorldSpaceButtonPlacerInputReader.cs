using System.Collections.Generic;
using Grid.Building;
using Grid.UI;
using Sirenix.OdinInspector;
using TurnBased;
using UnityEngine;
using UnityEngine.UI;

namespace Grid.ButtonInput {
    [RequireComponent(typeof(GridBuildingSystem), typeof(BuildingUI))]
    public class WorldSpaceButtonPlacerInputReader : MonoBehaviour
    {
        [BoxGroup("References")]
        [SerializeField, Required] private TurnBasedBrain turnBasedBrain;
        
        [TabGroup("Canvases", "Building Canvas")]
        [SerializeField] private Button placeBuildingButton;
        [TabGroup("Canvases", "Building Canvas")]
        [SerializeField] private Button closeBuildingCanvasButton;
        [TabGroup("Canvases", "Building Canvas")]
        [SerializeField] private Button incrementBuildingButton;
        [TabGroup("Canvases", "Building Canvas")]
        [SerializeField] private Button decrementBuildingButton;
        [TabGroup("Canvases", "Building Canvas")]
        [SerializeField] private Button openBuildingInfoButton;

        [TabGroup("Canvases", "Upgrade Canvas")]
        [SerializeField] private Button sellBuildingButton;
        [TabGroup("Canvases", "Upgrade Canvas")]
        [SerializeField] private Button upgradeBuildingButton;
        [TabGroup("Canvases", "Upgrade Canvas")]
        [SerializeField] private Button openCurrentInfoButton;
        [TabGroup("Canvases", "Upgrade Canvas")]
        [SerializeField] private Button openUpgradeInfoButton;
        [TabGroup("Canvases", "Upgrade Canvas")]
        [SerializeField] private Button closeInfoButton;
        [TabGroup("Canvases", "Upgrade Canvas")]
        [SerializeField] private Button closeUpgradingCanvasButton;
        
        [TabGroup("Canvases", "Screen Space Canvas")]
        [SerializeField] private Button restartButton;
        
        private List<ButtonStatus> _buttonStatuses;
        
        private GridBuildingSystem _gridBuildingSystem;
        private BuildingUI _buildingUI;

        private void Awake() {
            _gridBuildingSystem = GetComponent<GridBuildingSystem>();
            _buildingUI = GetComponent<BuildingUI>();

            // Initialize the list of ButtonStatus objects
            _buttonStatuses = new List<ButtonStatus> {
                new ButtonStatus(placeBuildingButton, ButtonType.PlaceBuildingButton),
                new ButtonStatus(closeBuildingCanvasButton, ButtonType.CloseBuildingCanvasButton),
                new ButtonStatus(incrementBuildingButton, ButtonType.IncrementBuildingButton),
                new ButtonStatus(openBuildingInfoButton, ButtonType.OpenBuildingInfoButton),
                new ButtonStatus(sellBuildingButton, ButtonType.SellBuildingButton),
                new ButtonStatus(upgradeBuildingButton, ButtonType.UpgradeBuildingButton),
                new ButtonStatus(openUpgradeInfoButton, ButtonType.OpenUpgradeInfoButton),
                new ButtonStatus(closeInfoButton, ButtonType.CloseInfoButton)
            };
        }
        // Only Invoked on the first click
        public void AddExplosiveCallback(ButtonType buttonType, UnityEngine.Events.UnityAction callback) {
            foreach (var buttonStatus in _buttonStatuses) {
                if (buttonStatus.ButtonType == buttonType) {
                    buttonStatus.AddFirstClickCallback(callback);
                    break;
                }
            }
        }

        private void Start() {
            SetupBuildingCanvasButtons();
            SetupUpgradeCanvasButtons();
            SetupScreenSpaceCanvasButtons();
        }

        private void SetupScreenSpaceCanvasButtons() {
            restartButton.onClick.AddListener(turnBasedBrain.RestartGame);
        }

        #region Add Button Listeners

        private void SetupBuildingCanvasButtons()
        {
            placeBuildingButton.onClick.AddListener(_gridBuildingSystem.PlaceSelectedBuilding);
            closeBuildingCanvasButton.onClick.AddListener(_gridBuildingSystem.ResetSelectedCell);
            incrementBuildingButton.onClick.AddListener(() => _gridBuildingSystem.ChangeSelectedBuilding(true));
            decrementBuildingButton.onClick.AddListener(() => _gridBuildingSystem.ChangeSelectedBuilding(false));
            openBuildingInfoButton.onClick.AddListener(_gridBuildingSystem.SetupBuildingInformation);
        }

        private void SetupUpgradeCanvasButtons()
        {
            sellBuildingButton.onClick.AddListener(SellSelectedBuilding);
            closeUpgradingCanvasButton.onClick.AddListener(_gridBuildingSystem.ResetSelectedCell);
            upgradeBuildingButton.onClick.AddListener( _gridBuildingSystem.RequestUpgradeBuilding);
            openUpgradeInfoButton.onClick.AddListener(_gridBuildingSystem.OpenUpgradeInformation);
            openCurrentInfoButton.onClick.AddListener(_gridBuildingSystem.OpenCurrentBuildingInformation);
            closeInfoButton.onClick.AddListener(_buildingUI.CloseBuildingInfoCanvas);
        }

        #endregion

        private void SellSelectedBuilding() {
            _gridBuildingSystem.DeleteSelectedBuildingOnGrid();
            _gridBuildingSystem.ResetSelectedCell();
        }
    
        private void OnDestroy()
        {
            UnsubscribeBuildingCanvasButtons();
            UnsubscribeUpgradeCanvasButtons();
            UnsubscribeScreenSpaceCanvasButtons();
        }

        #region Remove Button Listeners

        private void UnsubscribeBuildingCanvasButtons()
        {
            placeBuildingButton.onClick.RemoveListener(_gridBuildingSystem.PlaceSelectedBuilding);
            closeBuildingCanvasButton.onClick.RemoveListener(_gridBuildingSystem.ResetSelectedCell);
            incrementBuildingButton.onClick.RemoveListener(() => _gridBuildingSystem.ChangeSelectedBuilding(true));
            decrementBuildingButton.onClick.RemoveListener(() => _gridBuildingSystem.ChangeSelectedBuilding(false));
            openBuildingInfoButton.onClick.AddListener(_gridBuildingSystem.SetupBuildingInformation);
        }

        private void UnsubscribeUpgradeCanvasButtons()
        {
            sellBuildingButton.onClick.RemoveListener(SellSelectedBuilding);
            closeUpgradingCanvasButton.onClick.RemoveListener(_gridBuildingSystem.ResetSelectedCell);
            upgradeBuildingButton.onClick.RemoveListener( _gridBuildingSystem.RequestUpgradeBuilding);
            openUpgradeInfoButton.onClick.RemoveListener(_gridBuildingSystem.OpenUpgradeInformation);
            openCurrentInfoButton.onClick.RemoveListener(_gridBuildingSystem.OpenCurrentBuildingInformation);
            closeInfoButton.onClick.RemoveListener(_buildingUI.CloseBuildingInfoCanvas);
        }
        
        private void UnsubscribeScreenSpaceCanvasButtons() {
            restartButton.onClick.RemoveListener(turnBasedBrain.RestartGame);
        }
        
        

        #endregion

        private class ButtonStatus {
            private Button Button { get; }
            public ButtonType ButtonType { get; }
            private bool IsFirstClick { get; set; }

            public ButtonStatus(Button button, ButtonType buttonType) {
                Button = button;
                ButtonType = buttonType;
                IsFirstClick = true;
            }
            
            /// <summary>
            /// Add A Callback to the Button that will only be called on the first click, after we immediately remove the listener
            /// </summary>
            /// <param name="callback"></param>
            public void AddFirstClickCallback(UnityEngine.Events.UnityAction callback) { // TODO: Refactor
                UnityEngine.Events.UnityAction action = null;
                action = () => {
                    FirstClickAction(callback);
                    Button.onClick.RemoveListener(action);
                };
                Button.onClick.AddListener(action);
            }
            
            public void AddSecondClickCallback(UnityEngine.Events.UnityAction callback) { // TODO: Refactor
                UnityEngine.Events.UnityAction action = null;
                IsFirstClick = true;
                action = () => {
                    FirstClickAction(callback);
                    Button.onClick.RemoveListener(action);
                };
                Button.onClick.AddListener(action);
            }

            private void FirstClickAction(UnityEngine.Events.UnityAction callback) {
                if (IsFirstClick) {
                    callback.Invoke();
                    IsFirstClick = false;
                }
            }
        }
        public enum ButtonType {
            PlaceBuildingButton,
            CloseBuildingCanvasButton,
            IncrementBuildingButton,
            OpenBuildingInfoButton,
            SellBuildingButton,
            UpgradeBuildingButton,
            OpenUpgradeInfoButton,
            CloseInfoButton
        }
    }
}