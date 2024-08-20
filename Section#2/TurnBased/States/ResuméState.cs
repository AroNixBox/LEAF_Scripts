using System;
using System.Collections;
using System.Collections.Generic;
using Grid;
using Grid.Building;
using Grid.Building.Currency;
using TurnBased.UI;
using UnityEngine;
using Utils;
using Utils.FSM;

namespace TurnBased.States {
    /// <summary>
    /// Spawns Popup UI to show the player what happend since the last turn
    /// </summary>
    public class ResuméState : IState {
        private readonly TurnBasedReferences _references;
        private readonly Grid<GridObject> _grid;
        private readonly GridBuildingSystem _gridBuildingSystem;
        private readonly ResumeUI _resumeUi;
        
        private int _satisfactionDifference;
        private Coroutine _tutorialPingCoroutine;
        
        public ResuméState(TurnBasedReferences references) {
            _grid = references.gridBuildingSystem.GetGrid();
            _references = references;
            _resumeUi = _references.resumeUI;
            _gridBuildingSystem = _references.gridBuildingSystem;
        }
        public void OnEnter() {
            // Calculate percentage change and money change
            if (_references.actionCardSo != null) { // From the second round on.
                var percentageChange = _references.actionCardSo.moneyDifference / 100f;
                var change = _references.moneyPerRound * percentageChange;
                _references.moneyPerRound += (int)Math.Round(change);
                _satisfactionDifference = _references.actionCardSo.satisfactionPercentageDifference;
            } else { // In the first round there was no action card
                _satisfactionDifference = _references.startingSatisfaction;
            }
        
            // Reset the ActionCard after using it
            _references.actionCardSo = null;
        
            // Get all Buildings on the Grid
            var buildingsOnGrid = _grid.GetCorrectBuildingDataOfAllObjectsPlacedOnGrid();
            int moneyFromBuildings = 0;
            int satisfactionFromBuildings = 0;
            List<BuildingUpgradePair> buildingUpgradePairs = new List<BuildingUpgradePair>();
        
            // For each building on the grid
            foreach (var building in buildingsOnGrid) {
                // Add their income and satisfaction increase to the total
                moneyFromBuildings += building.Modifiers.Income;
                satisfactionFromBuildings += building.Modifiers.SatisfactionIncrease;
                
                // Get all Upgrades for each Building on the Grid
                var allUpgrades = _gridBuildingSystem.GetAllBuildingUpgrades(building);
                // Create a BuildingUpgradePair and add all upgrades of this building and the current upgrade to the Class
                buildingUpgradePairs.Add(new BuildingUpgradePair {
                    Upgrades = allUpgrades,
                    CurrentUpgrade = building
                });
            }
        
            // Publish the Pairs to the UI, in order to Display the Modifier Details
            _resumeUi.DisplayModifiersDetails(buildingUpgradePairs, _references.moneyPerRound, _satisfactionDifference);
        
            // Calculate the total Money and Satisfaction Increase and publish it to the Portfolio
            var earnedMoneytotal = _references.moneyPerRound + moneyFromBuildings;
            CurrencyPortfolio.Instance.EarnMoney(earnedMoneytotal);
            var totalSatisfactionDifference = _satisfactionDifference + satisfactionFromBuildings;
            CurrencyPortfolio.Instance.ChangeSatisfaction(totalSatisfactionDifference);
        
            // Update Resumé UI- Headers
            _resumeUi.DisplayTurnBasedIncome(earnedMoneytotal, _references.buildingUI.TextData.MoneyIncrease);
            _resumeUi.DisplayTurnBasedSatisfaction(totalSatisfactionDifference, _references.buildingUI.TextData.SatisfactionIncrease);
        
            // Increase Turn and Enable Popup
            _references.IncreaseTurn();
            _references.resumeUI.gameObject.SetActive(true);
        }

        public void Tick() {
            // Maybe discard all player input except for the one button to close the popup
        }

        public void OnExit() {
            // Despawn Popup
            _references.resumeUI.gameObject.SetActive(false);
        
            // Disable the action Button Flag
            _references.ResuméButtonPressed = false;
            
            if (_references.IsFirstTurn && _tutorialPingCoroutine != null) { // Stop the Tutorial Coroutine
                _references.StopCoroutine(_tutorialPingCoroutine);
            }
        }

        public Color GizmoState() {
            throw new NotImplementedException();
        }
    }
    
    public class BuildingUpgradePair {
        //All Upgrades of a Builing including the Base Upgrade
        public List<BuildingUpgrade> Upgrades { get; set; }
        //The Current Upgrade of the Building
        //This is stupid, could just pass an index and then take if fromthe Upgrades above, but fuck it XD
        public BuildingUpgrade CurrentUpgrade { get; set; }
    }
}