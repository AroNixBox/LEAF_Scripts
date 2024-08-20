using System;
using Sirenix.OdinInspector;
using TMPro;
using TurnBased.States;
using TurnBased.States.Tutorial;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils.FSM;

namespace TurnBased {
    [RequireComponent(typeof(TurnBasedReferences))]
    public class TurnBasedBrain : MonoBehaviour
    {
        [BoxGroup("References")]
        [SerializeField] private SceneField narrativeLevel;
        [BoxGroup("References")]
        [SerializeField] private TextMeshProUGUI debugText;
        
        private StateMachine _stateMachine;
        private StateMachine _tutorialStateMachine;
        private TurnBasedReferences _references;
        private void Awake()
        {
            _references = GetComponent<TurnBasedReferences>();
        }

        private void Start() {
            // Turn Based State Machine //
            
            _stateMachine = new StateMachine();
        
            void At(IState from, IState to, Func<bool> condition) => _stateMachine.AddTransition(from, to, condition);
            // void Any(IState to, Func<bool> condition) => _stateMachine.AddAnyTransition(to, condition);
            var resuméState = new ResuméState(_references);
            var actionState = new ActionState(_references);
            var buildState = new BuildState(_references);
            var gameOverState = new GameOverState(_references);

            // Normal Game Loop
            At(resuméState, actionState, () => _references.ResuméButtonPressed && !_references.IsLastTurn);
            At(actionState, buildState, () => _references.actionCardSo);
            At(buildState, resuméState, () => _references.NextTurnButtonPressed && !_references.IsLastTurn);
            
            // Last Turn
            At(resuméState, buildState, () => _references.ResuméButtonPressed && _references.IsLastTurn);
            At(buildState, gameOverState, () => _references.NextTurnButtonPressed && _references.IsLastTurn);
        
            // Popup in first round shows only the round money increase
            _stateMachine.SetState(resuméState);
            
            
            // Tutorial State Machine //
            
            _tutorialStateMachine = new StateMachine();
            
            void AtTutorial(IState from, IState to, Func<bool> condition) => _tutorialStateMachine.AddTransition(from, to, condition);
            void AtAnyTutorial(IState to, Func<bool> condition) => _tutorialStateMachine.AddAnyTransition(to, condition);
            
            var resumeSubStates = new ResumeSubStates(_references);
            var actionSubStates = new ActionSubStates(_references);
            var buildSubStates = new BuildSubStates(_references, buildState);
            var endState = new EmptyEndState(_tutorialStateMachine); // Self Destruction of the State Machine
            
            AtTutorial(resumeSubStates, actionSubStates, () => _references.IsFirstTurn && 
                                                               _stateMachine.IsCurrentState(actionState));
            
            AtTutorial(actionSubStates, buildSubStates, () => _references.IsFirstTurn && 
                                                             _stateMachine.IsCurrentState(buildState));
            
            
            AtTutorial(buildSubStates, endState, () => !_references.IsFirstTurn); // Self Destruction of the State Machine
            
            _tutorialStateMachine.SetState(resumeSubStates);
        }

        private void Update() {
            _stateMachine?.Tick();
            _tutorialStateMachine?.Tick();

            if (debugText == null) return;
            string currentStateDebugText = $"Current State: {_stateMachine?.GetCurrentStateName()}";
            if(debugText.text == currentStateDebugText) return;
            debugText.text = currentStateDebugText;
        }

        public void RestartGame() => SceneManager.LoadScene(narrativeLevel.SceneName);
    }
}
