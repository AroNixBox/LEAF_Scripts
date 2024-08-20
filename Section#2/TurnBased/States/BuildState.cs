using Grid.Building;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils.FSM;

namespace TurnBased.States {
    /// <summary>
    /// Player is able to build on the grid
    /// </summary>
    public class BuildState : IState {
        private readonly TurnBasedReferences _references;
        private readonly GridBuildingSystem _gridBuildingSystem;
        private readonly TouchInputManager _touchInputManager;

        public bool HasClickedOnGridInitially { get; private set; }

        public BuildState(TurnBasedReferences references) {
            _references = references;
            _gridBuildingSystem = _references.gridBuildingSystem;
            _touchInputManager = _references.touchInputManager;
        }

        public void OnEnter() {
            // Enable the Next Turn Panel
            _references.nextTurnPanel.gameObject.SetActive(true);
            _references.playerEvents.TurnCount();
        }
        /// <summary>
        /// Tracks Mouse Input to select a Cell
        /// </summary>
        public void Tick() {
            if (_touchInputManager.canBuild 
                && _touchInputManager.infoPanel.activeSelf == false) {
                Debug.Log("Tap");
                bool selectedACell = _gridBuildingSystem.SelectCell();

                if (selectedACell && !HasClickedOnGridInitially) {
                    HasClickedOnGridInitially = true;
                }
                _touchInputManager.canBuild = false;
            }

            _touchInputManager.canMove = true;
            _touchInputManager.Zooming();
        }

        public void OnExit() {
            _references.nextTurnPanel.gameObject.SetActive(false);
        
            // Unselect any sell and close the UI:
            _references.gridBuildingSystem.ResetSelectedCell();
        
            // Reset both build Button Flag
            _references.NextTurnButtonPressed = false;

            _touchInputManager.canMove = false;
        }

        public Color GizmoState() {
            throw new System.NotImplementedException();
        }
    }
}