using Sirenix.OdinInspector;
using TMPro;
using TurnBased.Data;
using TurnBased.Symbols;
using UnityEngine;
using UnityEngine.UI;

namespace TurnBased {
    /// <summary>
    /// Drag on top of an Action Card prefab to make it clickable.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ActionCard : MonoBehaviour {
        [SerializeField, Required] private Image visual;
        [SerializeField, Required] private TMP_Text actionName;
        [SerializeField, Required] private TMP_Text goldRateText;
        [SerializeField, Required] private TMP_Text satisfactionRateText;
        [SerializeField, Required, InlineEditor] private ActionCardSO actionCardSo;
        private Button _button;

        private void Awake() {
            _button = GetComponent<Button>();
            visual.sprite = actionCardSo.actionCardSprite;
            actionName.text = actionCardSo.ActionName;
        }

        private string GetColorCode(string prefix) {
            // Define color codes
            string red = "#FF0000";
            string green = "#00FF00";
            string white = "#FFFFFF";

            // Return color code based on prefix
            if (prefix == "+") return green;
            if (prefix == "") return red;
            return white; // return white if the prefix is "=" (unchanged)
        }
        private void Start() {
            _button.onClick.AddListener(OnButtonClicked);
            
            var goldPrefix = actionCardSo.moneyDifference > 0 ? "+" : actionCardSo.moneyDifference < 0 ? "" : "=";
            var goldRateColorCode = GetColorCode(goldPrefix);
            var goldSymbol = SymbolDictionary.CurrencySymbol;
            goldRateText.text = $"Money: <color={goldRateColorCode}>{goldPrefix}{actionCardSo.moneyDifference}{goldSymbol}</color>";
            
            var satisfactionPrefix = actionCardSo.satisfactionPercentageDifference > 0 ? "+" : actionCardSo.satisfactionPercentageDifference < 0 ? "" : "=";
            var satisfactionColorCode = GetColorCode(satisfactionPrefix);
            var percentageSymbol = SymbolDictionary.PercentageSymbol;
            satisfactionRateText.text = $"Satisfaction: <color={satisfactionColorCode}>{satisfactionPrefix}{actionCardSo.satisfactionPercentageDifference}{percentageSymbol}</color>";
        }
        private void OnButtonClicked() {
            // Pass the TurnBasedReferences this actionCard to copy the data and go into the next state!
            UIEvents.NewActionData.Get().Invoke(actionCardSo);
        }
        private void OnDestroy() {
            _button.onClick.RemoveListener(OnButtonClicked);
        }
    }
}
