using System;
using UnityEngine;
using Utils.FSM;

namespace TurnBased.States.Tutorial {
    public class PingState : IState {
        private readonly TurnBasedReferences _references;
        private readonly int _index;
        
        public PingState(TurnBasedReferences references, int index) {
            _references = references;
            _index = index;
        }

        public void OnEnter() {
            _references.IncreaseTutorialHint(_index); // Gold TMP
        }

        public void Tick() {
            // NO OP
        }

        public void OnExit() {
            // NO OP
        }

        public Color GizmoState() {
            throw new NotImplementedException();
        }
    }
}