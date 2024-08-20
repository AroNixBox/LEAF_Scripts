using System;
using UnityEngine;
using Utils.FSM;

namespace TurnBased.States.Tutorial {
    public class ActionSubStates : ISubStates {
        private readonly TurnBasedReferences _references;
        private StateMachine _actionSubStateMachine;
        private readonly IState _startState;

        public ActionSubStates(TurnBasedReferences references) {
            _references = references;
            
            _actionSubStateMachine = new StateMachine();
            
            void At(IState from, IState to, Func<bool> condition) => _actionSubStateMachine.AddTransition(from, to, condition);

            // Action State
            var explainActionCards = new PingState(_references, 3);
            var explainFirstCard = new PingState(_references, 4);
            var explainSecondCard = new PingState(_references, 5);
            
            _startState = explainActionCards;
            
            At(explainActionCards, explainFirstCard, () => Input.GetMouseButtonDown(0));
            At(explainFirstCard, explainSecondCard, () => Input.GetMouseButtonDown(0));
        }

        public void OnEnter() {
            _actionSubStateMachine.SetState(_startState);
        }

        public void Tick() {
            _actionSubStateMachine?.Tick();
        }

        public void OnExit() {
            Shutdown();
        }

        public Color GizmoState() {
            throw new NotImplementedException();
        }

        public string GetStateName() {
            return _actionSubStateMachine.GetCurrentStateName();
        }
        
        public void Shutdown() {
            _actionSubStateMachine.Shutdown();
            _actionSubStateMachine = null;
        }
    }
}