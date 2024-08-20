using System;
using UnityEngine;
using Utils.FSM;

namespace TurnBased.States.Tutorial {
    public class EmptyEndState : IState {
        private readonly StateMachine _stateMachine;
        public EmptyEndState(StateMachine stateMachine) {
            _stateMachine = stateMachine;
        }
        public void OnEnter() {
            _stateMachine.Shutdown();
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