using Sirenix.OdinInspector;
using UnityEngine;

namespace TurnBased.Data.TextData
{
    [CreateAssetMenu(fileName = "TutorialInfo", menuName = "TurnBased/TextData")]
    public class TextData : ScriptableObject {
        // Strings for the UI
        [field: SerializeField] public string MoneyIncrease { get; private set; } = "Money Increase";
        [field: SerializeField] public string SatisfactionIncrease { get; private set; } = "Satisfaction Rate Increase";
        [field: SerializeField] public string HeaderInformationCurrentBuilding { get; private set; } = "Current Building";
        [field: SerializeField] public string HeaderInformationUpgrade { get; private set; } = "Upgrade";
        [field: SerializeField] public string HeaderInformationNoUpgrade { get; private set; } = "No Upgrades Available";
        
        [field: SerializeField] public string BuildingNameHeader { get; private set; } = "Name"; 
        [field: SerializeField] public string BuildingCostHeader { get; private set; } = "Cost";
        [field: SerializeField] public string BuildingDescriptionHeader { get; private set; } = "Details";
        [field: SerializeField] public string BuildingIncomeHeader { get; private set; } = "Income Per Round";
        [field: SerializeField] public string BuildingSatisfactionHeader { get; private set; } = "Satisfaction Increase Per Round";
    }
}
