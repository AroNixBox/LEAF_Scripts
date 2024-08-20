using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Grid.Building {
    [CreateAssetMenu(menuName = "TurnBased/ Placed Object Type")]
    public class PlacedObjectTypeSO : ScriptableObject {
        [BoxGroup("General")]
        [ShowInInspector]
        [HideLabel]
        public BuildingUpgrade BaseBuildingInformation;
    
        [BoxGroup("Dimensions")]
        [Range(1, 10)]
        public int width = 1;
    
        [BoxGroup("Dimensions")]
        [Range(1, 10)]
        public int height = 1;
    
        [BoxGroup("Upgrades")]
        [InfoBox("Leave the list empty if you don't want any upgrades for this building.")]
        public List<BuildingUpgrade> upgrades = new ();
    
        public bool HasUpgrades() => upgrades.Count > 0;

        // List of Cells that will be Occupied by that building
        public List<Vector2Int> GetGridPositionList(Vector2Int offset /* Offset of position where we are clicking on */) {
            List<Vector2Int> gridPositionList = new List<Vector2Int>();

            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    gridPositionList.Add(new Vector2Int(x + offset.x, y + offset.y));
                }
            }
            return gridPositionList;
        }

        public uint GetBuildingCost() {
            return BaseBuildingInformation.Cost;
        }

        //The next Upgrade is always the current one, because initially the building is not upgraded
        public BuildingUpgrade GetNextUpgrade(int currentUpgradeIndex) {
            if (currentUpgradeIndex < upgrades.Count) {
                return upgrades[currentUpgradeIndex];
            }
        
            Debug.LogWarning("No more upgrades available for this building.");
            return null;
        }
    }
}