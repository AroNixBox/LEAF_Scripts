using System;
using Grid.ButtonInput;
using UnityEngine;
using Utils.FSM;

namespace TurnBased.States.Tutorial {
    public class BuildSubStates : ISubStates {
        private readonly TurnBasedReferences _references;
        private StateMachine _buildSubStateMachine;
        private readonly IState _startState;

        public BuildSubStates(TurnBasedReferences references, BuildState buildState) {
            _references = references;
            
            _buildSubStateMachine = new StateMachine();
            
            void At(IState from, IState to, Func<bool> condition) => _buildSubStateMachine.AddTransition(from, to, condition);
            
            // ButtonType bekommt die Macht erst in dem State, das darf nicht confusen! Der eingegebene Button ist der Button der in dem state aktiviert wird, Der Press ist die eigene Exit Condition für den State
            // Number is always one less than in BuildState, because Macht is given now for LATER and hint is shown NOW

            var pingGround = new PingState(_references, 6); // Ping the ground
            var pingBuildingInfo = new ButtonPingState(_references, WorldSpaceButtonPlacerInputReader.ButtonType.OpenBuildingInfoButton, 7); // Ping the info button
            var pingexplainBuildingInfo = new PingState(_references, 8); // Explain the building info
            var pingCloseInfoButton = new ButtonPingState(_references, WorldSpaceButtonPlacerInputReader.ButtonType.CloseInfoButton, 9); // Ping the close info button
            var pingincrementBuilding = new ButtonPingState(_references, WorldSpaceButtonPlacerInputReader.ButtonType.IncrementBuildingButton, 10); // Ping the increment building button
            var pingPlaceBuilding = new ButtonPingState(_references, WorldSpaceButtonPlacerInputReader.ButtonType.PlaceBuildingButton, 11); // Place the building
            var pingUpgradeInfo = new ButtonPingState(_references, WorldSpaceButtonPlacerInputReader.ButtonType.OpenUpgradeInfoButton, 12); // Ping the upgrade info button
            var pingexplainUpgradeInfo = new PingState(_references, 13); // Explain the upgrade info
            var pingCloseUpgradeInfo = new ButtonPingState(_references, WorldSpaceButtonPlacerInputReader.ButtonType.CloseInfoButton, 14); // Close the upgrade info
            var pingEndTutorial = new PingState(_references, 15); // End the tutorial
            
            // Straight pathway
            At(pingGround, pingBuildingInfo, () => buildState.HasClickedOnGridInitially);
            At(pingBuildingInfo, pingexplainBuildingInfo, () => pingBuildingInfo.ButtonPressed());
            At(pingexplainBuildingInfo, pingCloseInfoButton, () => _references.Tap);
            At(pingCloseInfoButton, pingincrementBuilding, () => pingCloseInfoButton.ButtonPressed());
            At(pingincrementBuilding, pingPlaceBuilding, () => pingincrementBuilding.ButtonPressed());
            At(pingPlaceBuilding, pingUpgradeInfo, () => pingPlaceBuilding.ButtonPressed());
            At(pingUpgradeInfo, pingexplainUpgradeInfo, () => pingUpgradeInfo.ButtonPressed());
            At(pingexplainUpgradeInfo, pingCloseUpgradeInfo, () => _references.Tap);
            At(pingCloseUpgradeInfo, pingEndTutorial, () => _references.Tap);
            
            // Shortcuts:
            At(pingBuildingInfo, pingPlaceBuilding, () => pingincrementBuilding.ButtonPressed());
            At(pingBuildingInfo, pingUpgradeInfo, () => pingPlaceBuilding.ButtonPressed());
            At(pingincrementBuilding,pingUpgradeInfo, () => pingPlaceBuilding.ButtonPressed());
            
            _startState = pingGround;
        }

        public void OnEnter() {
            _buildSubStateMachine.SetState(_startState);
        }

        public void Tick() {
            _buildSubStateMachine?.Tick();
        }

        public void OnExit() {
            _references.DestroyTutorialHint();
            Shutdown();
        }

        public Color GizmoState() {
            throw new NotImplementedException();
        }

        public string GetStateName() {
            return _buildSubStateMachine.GetCurrentStateName();
        }

        public void Shutdown() {
            _buildSubStateMachine.Shutdown();
            _buildSubStateMachine = null;
        }
    }
}