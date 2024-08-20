using UnityEngine;

namespace Grid.Building {
    [System.Serializable]
    public class Modifiers {
        [Range(-100, 100)] public int SatisfactionIncrease;
        [field: SerializeField] public int Income { get; set; }
    }
}