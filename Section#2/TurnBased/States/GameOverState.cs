using Grid.Building.Currency;
using Grid.UI;
using UnityEngine;
using Utils.FSM;

namespace TurnBased.States {
    public class GameOverState : IState{
        private TurnBasedReferences _references;
        private BuildingUI _buildingUI;
        private string _endDescriptionText = "This is a Placeholder, there could be a summary based on the persona and how the player ferormed in the game.";
        public GameOverState(TurnBasedReferences references) {
            _references = references;
            _buildingUI = references.buildingUI;
        }
        public void OnEnter() {
            _buildingUI.ShowQuitPanel(CurrencyPortfolio.Instance.GetCurrentMoney(), CurrencyPortfolio.Instance.GetCurrentSatisfaction(), _endDescriptionText);
        }

        public void Tick() {
            
        }

        public void OnExit() {
            // Close Game Over Popup
            // Cleanup
        }

        public Color GizmoState() {
            throw new System.NotImplementedException();
        }
    }
}