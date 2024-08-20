using UnityEngine;
using Utils.FSM;

namespace TurnBased.States {
    /// <summary>
    /// Player can select an Action to perform
    /// </summary>
    public class ActionState : IState {
        private readonly TurnBasedReferences _references;
    
        public ActionState(TurnBasedReferences references) {
            _references = references;
        }
        public void OnEnter() {
            
            // Enable Action Popup
            _references.actionPopup.gameObject.SetActive(true);
        }
        public void Tick() {
            // Maybe discard all player input except for the one button to close the popup
        }

        public void OnExit() {
            // Which Button was pressed, increase or decrease Satisfaction / Money for next Round
        
            // Despawn Popup
            _references.actionPopup.gameObject.SetActive(false);
        
            // Even tho ActionDataSo is the flag for this state, we reset it before exiting the ResuméState, due to we need the data!
        }

        public Color GizmoState() {
            throw new System.NotImplementedException();
        }
    }
}