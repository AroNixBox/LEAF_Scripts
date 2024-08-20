using System;
using UnityEngine;
using Utils.FSM;

namespace TurnBased.States.Tutorial {
    public class ResumeSubStates : ISubStates {
        private readonly IState _startState;
        private StateMachine _resumeSubStateMachine;

        public ResumeSubStates(TurnBasedReferences references) {
            _resumeSubStateMachine = new StateMachine();
            
            void At(IState from, IState to, Func<bool> condition) => _resumeSubStateMachine.AddTransition(from, to, condition);

            var pingGold = new PingState(references, 0);
            var pingSatisfaction = new PingState(references, 1);
            var pingContinueButton = new PingState(references, 2);
            
            _startState = pingGold;
            
            At(pingGold, pingSatisfaction, () => Input.GetMouseButtonDown(0));
            At(pingSatisfaction, pingContinueButton, () => Input.GetMouseButtonDown(0));
        }

        public void OnEnter() {
            _resumeSubStateMachine.SetState(_startState);
        }

        public void Tick() {
            _resumeSubStateMachine?.Tick();
        }

        public void OnExit() {
            Shutdown();
        }

        public Color GizmoState() {
            throw new NotImplementedException();
        }

        public string GetStateName() {
            return _resumeSubStateMachine.GetCurrentStateName();
        }
        public void Shutdown() {
            _resumeSubStateMachine.Shutdown();
            _resumeSubStateMachine = null;
        }
    }
}