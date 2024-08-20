using Data.Tutorial;
using Grid.Building;
using Grid.ButtonInput;
using Grid.UI;
using Sirenix.OdinInspector;
using TMPro;
using TurnBased.Data;
using TurnBased.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TurnBased {
    public class TurnBasedReferences : MonoBehaviour
    {
        [BoxGroup("References")] public GridBuildingSystem gridBuildingSystem;
        [BoxGroup("References")] public PlayerEvents playerEvents;
        [BoxGroup("References")] public Typologie typologie;
        [BoxGroup("References")] public TouchInputManager touchInputManager;
        [BoxGroup("References")] public PlayerInput playerInput;
        private InputAction inputAction;

        [BoxGroup("References")] public BuildingUI buildingUI; 
        // I only need this for the tutorial... TODO: Find a better way to do this
        [BoxGroup("References")] public WorldSpaceButtonPlacerInputReader buttonInputs;
        
        [BoxGroup("Tutorial")] [InlineEditor] public TutorialInfo tutorialHintData;
        [BoxGroup("Tutorial")] [SerializeField] private Canvas ScreenSpaceCanvas;
        [BoxGroup("Tutorial")] [SerializeField] private float tutorialBackgroundPadding = 50;

        [BoxGroup("Popup")] public Transform actionPopup;
        [BoxGroup("Popup")] [ReadOnly] public ActionCardSO actionCardSo;
    
        [BoxGroup("Popup")] public ResumeUI resumeUI;
        [BoxGroup("Popup")] public Button resuméCloseButton;
        public bool ResuméButtonPressed { get; set; }
    
        [BoxGroup("Popup")] public Transform nextTurnPanel;
        [BoxGroup("Popup")] public Button nextTurnButton;
        public bool NextTurnButtonPressed { get; set; }
    
        [BoxGroup("Game Settings")] [MinValue(0)] public int moneyPerRound = 8000;
        [BoxGroup("Game Settings")] [MinValue(0)] public int startingSatisfaction = 20;
        [BoxGroup("Game Settings")] [MinValue(1)] public int maxTurns = 12;
    
        private int _currentTurn;
        private Vector2 originalTutorialBackgroundPosition;

        public bool IsLastTurn => _currentTurn == maxTurns;
        
        public bool IsFirstTurn => _currentTurn == 1; // We increase every resume state, if we are in turn 1 means we are in our first round.
        public bool Tap => inputAction.triggered;
        private RectTransform _tutorialHint;
        private Image _tutorialHintTextBackGround;
        private TMP_Text _tutorialHintText;


        private void OnEnable() {
            UIEvents.NewActionData.Get().AddListener(UpdateActionData);
        }

        private void UpdateActionData(ActionCardSO newActionData) {
            actionCardSo = newActionData;
        }

        private void Start() {
            resuméCloseButton.onClick.AddListener(() => ResuméButtonPressed = true);
            nextTurnButton.onClick.AddListener(() => NextTurnButtonPressed = true);

            inputAction = playerInput.actions.FindAction("Touch");
        }

        private void OnDestroy() {
            resuméCloseButton.onClick.RemoveListener(() => ResuméButtonPressed = true);
            nextTurnButton.onClick.RemoveListener(() => NextTurnButtonPressed = true);
            UIEvents.NewActionData.Get().RemoveListener(UpdateActionData);
        }
    
        public void IncreaseTurn() {
            _currentTurn++;
        }

        // Nullable variable to store the type of the last tutorial hint created
        private TutorialInfo.TutorialInfoData.TutorialHintType? _lastHintType;
        
        // Nullable variable to store the last text orientation of the tutorial hint created
        private TutorialInfo.TutorialInfoData.TextOrientation? _lastTextOrientation;

        // The Current Tutorial Hint Data
        private TutorialInfo.TutorialInfoData _tutorialInfoData;
        
        /*
         * Method to set the base for the tutorial hint. It checks if a hint has been created before and if the new hint type is different from the last one.
         * If it's the first hint or the hint type has changed, it destroys the old hint and creates a new one.
         * It also sets the parent, position, and text of the hint, and adjusts the size of the hint background to fit the text.
         */
        public void IncreaseTutorialHint(int hintIndex) {
            var newHintType = tutorialHintData.tutorialData[hintIndex].hintType;
            var newTextOrientation = tutorialHintData.tutorialData[hintIndex].textOrientation;
        
            // If we set the hint for the first time, or if the new hint type is different from the last one, we have to instantiate it.
            if(_tutorialHint == null || newHintType != _lastHintType || newTextOrientation != _lastTextOrientation) {
                // Destroy the old hint if it exists
                if (_tutorialHint != null) {
                    Destroy(_tutorialHint.gameObject);
                }
        
                // Create the tutorial hint
                _tutorialInfoData = tutorialHintData.CreateTutorialHint(hintIndex);
                _tutorialHint = Instantiate(_tutorialInfoData.hintObject).GetComponent<RectTransform>();
                
                // Get the TextObject
                _tutorialHintText = _tutorialHint.GetComponentInChildren<TMP_Text>();
        
                // Get the Background Image of the Text
                _tutorialHintTextBackGround = _tutorialHint.GetComponentInChildren<Image>();
                originalTutorialBackgroundPosition = _tutorialHintTextBackGround.rectTransform.anchoredPosition;
        
                // Store the new hint type
                _lastHintType = newHintType;
                _lastTextOrientation = newTextOrientation;
            }
        
            // Set the parent based on the hint type
            _tutorialHint.SetParent(_tutorialInfoData.hintType switch {
                TutorialInfo.TutorialInfoData.TutorialHintType.ScreenSpace => ScreenSpaceCanvas.transform,
                TutorialInfo.TutorialInfoData.TutorialHintType.WorldSpace_Raw => buildingUI.BuildingPanel.parent,
                TutorialInfo.TutorialInfoData.TutorialHintType.WorldSpace_Build => buildingUI.BuildingPanel,
                TutorialInfo.TutorialInfoData.TutorialHintType.WorldSpace_Upgrade => buildingUI.UpgradePanel,
                _ => throw new System.ArgumentOutOfRangeException() // This should never happen
            });
        
            // Set the position if provided
            _tutorialHint.anchoredPosition = tutorialHintData.tutorialData[hintIndex].anchorPosition;
        
            _tutorialHintText.text = hintIndex < tutorialHintData.tutorialData.Count ?
                tutorialHintData.tutorialData[hintIndex].hintText
                : "No Hint available";
        
            _tutorialHintText.ForceMeshUpdate(); // Force the text to update its layout immediately instead of waiting for the next frame
        
            // Update the size of the tutorialHintTextBackground to match the size of the tutorialHintText
            var backgroundRect = _tutorialHintTextBackGround.rectTransform;
        
            // Use bounds to get the actual width and height of the text
            var textBounds = _tutorialHintText.bounds;
        
            backgroundRect.sizeDelta = new Vector2(textBounds.size.x + tutorialBackgroundPadding, textBounds.size.y + tutorialBackgroundPadding);
            
            // Move the background image half the padding to the left or right based on the text orientation
            backgroundRect.anchoredPosition = _tutorialInfoData.textOrientation switch {
                TutorialInfo.TutorialInfoData.TextOrientation.Left => new Vector2(originalTutorialBackgroundPosition.x + tutorialBackgroundPadding * 0.5f, originalTutorialBackgroundPosition.y),
                TutorialInfo.TutorialInfoData.TextOrientation.Right => new Vector2(originalTutorialBackgroundPosition.x - tutorialBackgroundPadding * 0.5f, originalTutorialBackgroundPosition.y),
                _ => backgroundRect.anchoredPosition
            };
        }
        public void DestroyTutorialHint() {
            Destroy(_tutorialHint.gameObject);
        }
    }
}