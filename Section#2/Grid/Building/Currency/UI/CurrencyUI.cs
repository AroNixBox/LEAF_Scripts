using System;
using Sirenix.OdinInspector;
using TMPro;
using TurnBased.Symbols;
using UnityEngine;

namespace Grid.Building.Currency {
    public class CurrencyUI : MonoBehaviour {
        [TabGroup("UI", "Money")]
        [SerializeField] private string representMoneyText = "Current Money";
        [TabGroup("UI", "Money")]
        [SerializeField] private TMP_Text moneyText;
        
        [TabGroup("UI", "Satisfaction")]
        [SerializeField] private string representSatisfactionText = "Satisfaction Rate";
        [TabGroup("UI", "Satisfaction")]
        [SerializeField] private TMP_Text satisfactionText;
        private void Start() {
            CurrencyPortfolio.Instance.OnMoneyChanged += UpdateMoneyText;
            CurrencyPortfolio.Instance.OnSatisfactionPercentageChanged += UpdateSatisfactionText;
        }
        private void UpdateMoneyText(int newMoneyAmount) {
            var currencySymbol = SymbolDictionary.CurrencySymbol;
            moneyText.text = $"{representMoneyText} {newMoneyAmount}{currencySymbol}";
        }
        private void UpdateSatisfactionText(int newSatisfactionPercentage) {
            var percentageSymbol = SymbolDictionary.PercentageSymbol;
            satisfactionText.text = $"{representSatisfactionText} {newSatisfactionPercentage}{percentageSymbol}";
        }
        private void OnDestroy() {
            CurrencyPortfolio.Instance.OnMoneyChanged -= UpdateMoneyText;
            CurrencyPortfolio.Instance.OnSatisfactionPercentageChanged -= UpdateSatisfactionText;
        }
    }
}
