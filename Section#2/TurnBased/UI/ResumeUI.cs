using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using TMPro;
using TurnBased.States;
using TurnBased.Symbols;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace TurnBased.UI {
    public class ResumeUI : MonoBehaviour
    {
        [BoxGroup("Text Fields")]
        [SerializeField] private TMP_Text totalIncomeText;

        [BoxGroup("Text Fields")]
        [SerializeField] private TMP_Text satisfactionChangedText;

        [BoxGroup("Text Fields")]
        [SerializeField] private TMP_Text incomeDetailsText;

        [BoxGroup("Text Fields")]
        [SerializeField] private TMP_Text satisfactionDetailsText;
        
        [BoxGroup("Modifiers Text Fields")]
        [Tooltip("Header for the Income Details")]
        [SerializeField] private string incomeDetailsHeader = "Income";
        
        [BoxGroup("Modifiers Text Fields")]
        [Tooltip("Header for the Satisfaction Details")]
        [SerializeField] private string satisfactionDetailsHeader = "Satisfaction";
        
        [BoxGroup("Modifiers Text Fields")]
        [Tooltip("Displayed in ModifierDetailsText's when there is no building on the Grid")]
        [SerializeField] private string noBuildingPlaced = "No Data";

        /// <summary>
        /// Set the Income Header Text
        /// </summary>
        /// <param name="totalChangeAmount"></param>
        /// <param name="sender"></param>
        public void DisplayTurnBasedIncome(int totalChangeAmount, string sender) {
            totalIncomeText.text = ChangeNumberFormat(totalChangeAmount, sender, SymbolDictionary.CurrencySymbol);
        }
        
        /// <summary>
        /// Set the Satisfaction Header Text
        /// </summary>
        /// <param name="changeAmount"></param>
        /// <param name="sender"></param>
        public void DisplayTurnBasedSatisfaction(int changeAmount, string sender) {
            satisfactionChangedText.text = ChangeNumberFormat(changeAmount, sender, SymbolDictionary.PercentageSymbol);
        }
        
        /// <summary>
        /// Changes Textcolor and prefix based on the value of changeAmount
        /// </summary>
        /// <param name="changeAmount"></param>
        /// <param name="sender"></param>
        /// <param name="symbol"></param>
        /// <returns></returns>
        private string ChangeNumberFormat(int changeAmount, string sender, char symbol) {
            var prefix = changeAmount > 0 ? "+" : changeAmount < 0 ? "" : "=";
            var colorCode = GetColorCode(prefix);
            string amountString = $"{prefix}{changeAmount}";
            return $"{sender}: <color={colorCode}>{amountString}{symbol}</color>";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buildingUpgradePairs"></param>
        /// <param name="moneyPerRound"></param>
        /// <param name="satisfactionDifference"></param>
        public void DisplayModifiersDetails(List<BuildingUpgradePair> buildingUpgradePairs, int moneyPerRound, int satisfactionDifference) {
            if (!buildingUpgradePairs.Any()) { // No Buildings on the Grid
                incomeDetailsText.text = noBuildingPlaced;
                satisfactionDetailsText.text = noBuildingPlaced;
                return;
            }

            //Buildings on Grid, display the ingredient details for Income and Satisfaction
            incomeDetailsText.text = FormatDetailsText(
                buildingUpgradePairs, 
                pair => pair.CurrentUpgrade.Modifiers.Income, 
                incomeDetailsHeader, 
                SymbolDictionary.CurrencySymbol, 
                moneyPerRound);
            
            satisfactionDetailsText.text = FormatDetailsText(
                buildingUpgradePairs, 
                pair => pair.CurrentUpgrade.Modifiers.SatisfactionIncrease, 
                satisfactionDetailsHeader, 
                SymbolDictionary.PercentageSymbol, 
                satisfactionDifference);
        }
        
        /// <summary>
        /// Formats the details text for the income or satisfaction changes caused by building upgrades.
        /// </summary>
        /// <param name="pairs"> Upgrade Pairs based on Buildings on the Grid.</param>
        /// <param name="valueSelector"> A function to select the value (income or satisfaction increase) from a building upgrade pair.</param>
        /// <param name="valueModifierName"> The name of the value modifier (income or satisfaction).</param>
        /// <param name="symbol"> The symbol representing the value (currency or percentage).</param>
        /// <param name="totalValue"> The total value change (income or satisfaction) for the round.</param>
        /// <returns> A string representing the formatted details text.</returns>
        private string FormatDetailsText(List<BuildingUpgradePair> pairs, Func<BuildingUpgradePair, int> valueSelector, string valueModifierName, char symbol, int totalValue) {
            // Group the pairs by the name of the first upgrade in the Upgrades list
            var groupedPairs = pairs.GroupBy(pair => pair.Upgrades[0].Name);
            // Initialize a new StringBuilder
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
        
            // Determine the color of the total value based on its sign
            var totalValueColor = GetColorCode(totalValue > 0 ? "+" : totalValue < 0 ? "" : "=");
            // Append the total value to the StringBuilder
            stringBuilder.Append($"<b>Roundbased {valueModifierName}:</b> <color={totalValueColor}>{totalValue}{symbol}</color>\n");
        
            // Iterate over each group of pairs
            foreach (var group in groupedPairs) {
                // Get the name of the base upgrade
                var baseUpgradeName = group.Key;
                // Convert the group to a list
                var pairsForThisBaseUpgrade = group.ToList();
                // Group the pairs by the name of the current upgrade
                var groupedCurrentUpgrades = pairsForThisBaseUpgrade.GroupBy(pair => pair.CurrentUpgrade.Name);
        
                // Append the base upgrade name to the StringBuilder
                stringBuilder.Append($"<b>{valueModifierName} from {baseUpgradeName}:</b> {{ ");
        
                // Iterate over each group of current upgrades, ordered by their index in the Upgrades list
                foreach (var currentUpgradeGroup in groupedCurrentUpgrades.OrderBy<IGrouping<string, BuildingUpgradePair>, int>(group => group.First().Upgrades.IndexOf(group.First().CurrentUpgrade))) {
                    // Get the name of the current upgrade
                    var currentUpgradeName = currentUpgradeGroup.Key;
                    // Convert the group to a list
                    var pairsForThisCurrentUpgrade = currentUpgradeGroup.ToList();
                    // Get the count of buildings
                    var buildingCount = pairsForThisCurrentUpgrade.Count;
                    // Get the value of the first pair in the list
                    var value = valueSelector(pairsForThisCurrentUpgrade.First());
                    // Determine the color of the value based on its sign
                    var valueColor = GetColorCode(value > 0 ? "+" : value < 0 ? "" : "=");
        
                    // Append the current upgrade details to the StringBuilder
                    stringBuilder.Append($"{{ {currentUpgradeName}: <color={valueColor}>{value}{symbol} x {buildingCount}</color> }}, ");
                }
        
                // If the StringBuilder has more than 2 characters, remove the last 2 characters
                if (stringBuilder.Length > 2) {
                    stringBuilder.Length -= 2;
                }
        
                // Append a closing brace and a newline to the StringBuilder
                stringBuilder.Append(" } \n");
            }
        
            // Return the string built by the StringBuilder
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Returns the color code based on the prefix
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        private string GetColorCode(string prefix) {
            string red = "#FF0000";
            string green = "#00FF00";
            string white = "#FFFFFF";

            if (prefix == "+") return green;
            if (prefix == "") return red;
            return white;
        }
    }
}