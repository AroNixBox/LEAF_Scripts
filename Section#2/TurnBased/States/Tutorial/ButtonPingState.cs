using System;
using Grid.ButtonInput;
using UnityEngine;
using Utils.FSM;

namespace TurnBased.States.Tutorial {
    public class ButtonPingState : IState {
        private readonly TurnBasedReferences _references;
        private readonly int _tutorialIndex;
        private bool _canExit;
        private readonly bool _isSecondCall;
        public ButtonPingState(TurnBasedReferences references, WorldSpaceButtonPlacerInputReader.ButtonType buttonType, int tutorialIndex) {
            _references = references;
            _tutorialIndex = tutorialIndex;
            
            _references.buttonInputs.AddExplosiveCallback(buttonType, () => _canExit = true);
        }

        public void OnEnter() {
            _references.IncreaseTutorialHint(_tutorialIndex); // Gold TMP
        }
        public bool ButtonPressed() {
            return _canExit;
        }
        
        public void Tick() {
            
        }

        public void OnExit() {
            
        }

        public Color GizmoState() {
            throw new NotImplementedException();
        }
    }
}