using System;
using Grid.Building;
using Sirenix.OdinInspector;
using TMPro;
using TurnBased.Data.TextData;
using UnityEngine;
using UnityEngine.Serialization;

namespace Grid.UI {
    [RequireComponent(typeof(GridBuildingSystem))]
    public class BuildingUI : MonoBehaviour
    {
        [field: SerializeField] public TextData TextData { get; private set; }
        
        [TabGroup("Canvases", "Building Canvas")]
        [SerializeField] private GameObject buildingPanel;
        
        public Transform BuildingPanel => buildingPanel.transform;
        
        [TabGroup("Canvases", "Building Canvas")]
        [SerializeField] private TMP_Text costText;
        
        [TabGroup("Canvases", "Upgrading Canvas")]
        [SerializeField] private GameObject upgradePanel;
        
        public Transform UpgradePanel => upgradePanel.transform;
        
        [TabGroup("Canvases", "Upgrading Canvas")]
        [SerializeField] private GameObject upgradeInformationPanel;
        [TabGroup("Canvases", "Info Panel")]
        [SerializeField] private TMP_Text infoHeaderText;
        [TabGroup("Canvases", "Info Panel")] 
        [SerializeField] private TMP_Text infoNameText;
        [TabGroup("Canvases", "Info Panel")]
        [SerializeField] private TMP_Text infoCostText;
        [TabGroup("Canvases", "Info Panel")]
        [SerializeField] private TMP_Text infoDescriptionText;
        [TabGroup("Canvases", "Info Panel")]
        [SerializeField] private TMP_Text moneyPerRoundText;
        [TabGroup("Canvases", "Info Panel")]
        [SerializeField] private TMP_Text satisfactionPerRoundText;
        
        [TabGroup("Canvases", "Quit Panel")]
        [SerializeField] private GameObject quitPanel;
        [TabGroup("Canvases", "Quit Panel")]
        [SerializeField] private TMP_Text endIncomeText;
        [TabGroup("Canvases", "Quit Panel")]
        [SerializeField] private TMP_Text endSatisfactionText;
        [TabGroup("Canvases", "Quit Panel")]
        [SerializeField] private TMP_Text endDescriptionText;
        
    
        [BoxGroup("Text Colors")]
        [SerializeField] private Color canAffordTextColor = Color.green;
        
        [BoxGroup("Text Colors")]
        [SerializeField] private Color canNotAffordTextColor = Color.red;
    
        private GridBuildingSystem _gridBuildingSystem;

        private void Awake() {
            _gridBuildingSystem = GetComponent<GridBuildingSystem>();
        }

        private void Start()
        {
            _gridBuildingSystem.OnUpgradeMenuOpened += ShowUpgradeCanvas;
            _gridBuildingSystem.OnMoneyChanged += UpdateCostText;
            _gridBuildingSystem.OnBuildingMenuOpened += ShowBuildingCanvas;
            _gridBuildingSystem.OnStoppedBuilding += HideUI;
            _gridBuildingSystem.OnBuildingInfoChanged += SetInfoTexts;
        }

        private void ShowUpgradeCanvas(Vector3 position)
        {
            buildingPanel.gameObject.SetActive(false);
            upgradePanel.transform.position = position;
            upgradePanel.gameObject.SetActive(true);
        }

        /// <summary>
        /// Sets the Texts that display the Information of the building, either the current building or the upgrade
        /// </summary>
        /// <param name="buildingUpgrade"></param>
        /// <param name="upgradeInformation"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void SetInfoTexts(BuildingUpgrade buildingUpgrade, InformationType upgradeInformation)
        {
            // Set the header text based on the upgradeInformation
            infoHeaderText.text = upgradeInformation switch
            {
                InformationType.Current => TextData.HeaderInformationCurrentBuilding,
                InformationType.Upgrade => TextData.HeaderInformationUpgrade,
                InformationType.None => TextData.HeaderInformationNoUpgrade,
                _ => throw new ArgumentOutOfRangeException(nameof(upgradeInformation), upgradeInformation, null)
            };

            // Set the building information (name, cost, and description) or clear all strings based on upgradeInformation
            (infoNameText.text, infoCostText.text, infoDescriptionText.text, moneyPerRoundText.text, satisfactionPerRoundText.text) = upgradeInformation switch {
                InformationType.Current or InformationType.Upgrade => (
                    $"<b>{TextData.BuildingNameHeader}:</b> {buildingUpgrade.Name}",
                    $"<b>{TextData.BuildingCostHeader}:</b> {buildingUpgrade.Cost} €",
                    $"<b>{TextData.BuildingDescriptionHeader}:</b> {buildingUpgrade.Description}",
                    $"<b>{TextData.BuildingIncomeHeader}:</b> {buildingUpgrade.Modifiers.Income} €",
                    $"<b>{TextData.BuildingSatisfactionHeader}:</b> {buildingUpgrade.Modifiers.SatisfactionIncrease} %"),
                _ => ( // There is No Upgrade available: Clear all strings
                    string.Empty, 
                    string.Empty, 
                    string.Empty, 
                    string.Empty, 
                    string.Empty)
            };

            // Activate the upgrade information panel
            upgradeInformationPanel.SetActive(true);
        }

        public void CloseBuildingInfoCanvas() {
            upgradeInformationPanel.SetActive(false);
        }
        
        /// <summary>
        /// Enables the Building Canvas and sets the position of the canvas
        /// </summary>
        /// <param name="position"></param>
        private void ShowBuildingCanvas(Vector3 position) {
            buildingPanel.gameObject.SetActive(true);
            buildingPanel.transform.position = position;
        }

        /// <summary>
        /// Sets the Cost text and changes the color based on if the player can afford the building or not
        /// </summary>
        /// <param name="canAfford"></param>
        /// <param name="cost"></param>
        private void UpdateCostText(bool canAfford, string cost)
        {
            costText.text = cost;
            costText.color = canAfford ? canAffordTextColor : canNotAffordTextColor;
        }

        /// <summary>
        /// Hides the Building and Upgrade Canvas
        /// </summary>
        private void HideUI() {
            buildingPanel.gameObject.SetActive(false);
            upgradePanel.gameObject.SetActive(false);
            CloseBuildingInfoCanvas();
        }
        
        public void ShowQuitPanel(int income, int satisfaction, string description) {
            endIncomeText.text = $"Income: {income} €";
            endSatisfactionText.text = $"Satisfaction: {satisfaction} %";
            endDescriptionText.text = description;
            quitPanel.SetActive(true);
        }
    
        private void OnDestroy() {
            _gridBuildingSystem.OnUpgradeMenuOpened -= ShowUpgradeCanvas;
            _gridBuildingSystem.OnMoneyChanged -= UpdateCostText;
            _gridBuildingSystem.OnBuildingMenuOpened -= ShowBuildingCanvas;
            _gridBuildingSystem.OnStoppedBuilding -= HideUI;
            _gridBuildingSystem.OnBuildingInfoChanged -= SetInfoTexts;
        }
    }
}
/// <summary>
/// Helps show the correct Header Text and to null the strings if there is no upgrade available
/// </summary>
public enum InformationType {
    None, Current, Upgrade
}
