using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Grid.Building.Currency {
    public class CurrencyPortfolio : MonoBehaviour
    {
        public static CurrencyPortfolio Instance { get; private set; }
        public event Action<int> OnMoneyChanged = delegate {  };
        public event Action<int> OnSatisfactionPercentageChanged = delegate {  };
        
        //Current Money
        private int _money;
        
        // Current Satisfaction
        private int _satisfactionPercentage;
        public void ChangeSatisfaction(int amount) {
            var clampAmount = Mathf.Clamp(_satisfactionPercentage + amount, 0, 100);
            _satisfactionPercentage = clampAmount;
            OnSatisfactionPercentageChanged?.Invoke(_satisfactionPercentage);
        }
        
        public int GetCurrentMoney() {
            return _money;
        }
        
        public int GetCurrentSatisfaction() {
            return _satisfactionPercentage;
        }
        
        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
            } else {
                Instance = this;
            }
        }

        public bool CanAfford(int cost) {
            return _money >= cost;
        }
        
        public void SpendMoney(int cost) {
            if (!CanAfford(cost)) {
                Debug.LogError("Not enough money to spend, but can afford was true");
                return;
            }
            _money -= cost;
            OnMoneyChanged?.Invoke(_money);
        }
        
        public void EarnMoney(int amount) {
            _money += amount;
            OnMoneyChanged?.Invoke(_money);
        }
        
        [HorizontalGroup("Debug Money")]
        [Button("Add Money", ButtonSizes.Medium, ButtonStyle.Box)]
        [GUIColor(0, 1, 0)]
        private void AddDebugMoney() {
            EarnMoney(50000);
        }

        [HorizontalGroup("Debug Money")]
        [Button("Remove Money", ButtonSizes.Medium, ButtonStyle.Box)]
        [GUIColor(1, 0, 0)]
        private void RemoveDebugMoney() {
            _money = 0;
            OnMoneyChanged?.Invoke(_money);
        }
        
        [HorizontalGroup("Satisfaction")]
        [Button("Add Satisfaction", ButtonSizes.Medium, ButtonStyle.Box)]
        [GUIColor(0, 1, 0)]
        private void AddSatisfaction() {
            // Increase the satisfaction by 10%
            ChangeSatisfaction(10);
        }
        
        [HorizontalGroup("Satisfaction")]
        [Button("Remove Satisfaction", ButtonSizes.Medium, ButtonStyle.Box)]
        [GUIColor(1, 0, 0)]
        private void RemoveSatisfaction() {
            // Decrease the satisfaction by 10%
            ChangeSatisfaction(-10);
        }
    }
}
