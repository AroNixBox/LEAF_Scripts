using Sirenix.OdinInspector;
using UnityEngine;

namespace Grid.Building {
    /// <summary>
    /// Data for a Building State/ Upgrade
    /// </summary>
    [System.Serializable]
    public class BuildingUpgrade {
        [BoxGroup("Upgrade Info")]
        [LabelWidth(100)]
        [field: SerializeField] public string Name { get; set; }
    
        [BoxGroup("Upgrade Info")]
        [LabelWidth(100)]
        [field: SerializeField] public uint Cost { get; set; }
    
        [BoxGroup("Upgrade Info")]
        [LabelWidth(100)]
        [MultiLineProperty(3)]
        [field: SerializeField] public string Description { get; set; }

        [BoxGroup("Upgrade Info")]
        [ShowInInspector]
        [HideLabel]
        public Modifiers Modifiers = new(); // New instance of Class !!!!

        [BoxGroup("Upgrade Prefab")]
        [HideLabel]
        [InfoBox("Pivot position needs to be on the bottom left corner!!!")]
        [field: SerializeField] public Transform Prefab { get; private set; }
        public bool IsAffordable(uint currentMoney) {
            return currentMoney >= Cost;
        }
    }
}