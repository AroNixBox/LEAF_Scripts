using UnityEngine;

namespace TurnBased.Data {
    /// <summary>
    /// Data that is passed to the TurnBasedReferences when a button is pressed.
    /// </summary>
    [CreateAssetMenu(fileName = "ActionCardSO", menuName = "TurnBased/ActionCardSO", order = 0)]
    public class ActionCardSO : ScriptableObject
    {
        [field: SerializeField] public string ActionName { get; private set; }
        
        public Sprite actionCardSprite;
        [Range(-100, 100)] public int moneyDifference;
        [Range(-100, 100)] public int satisfactionPercentageDifference;
    }
}
