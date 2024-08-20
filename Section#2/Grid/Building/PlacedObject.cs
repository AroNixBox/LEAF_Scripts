using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Grid.Building {
    public class PlacedObject : MonoBehaviour {
        public BuildingUpgrade PlacedBuildingInformation { get; } = new ();
    
        private int _currentUpgradeIndex;
    
        private PlacedObjectTypeSO _placedObjectTypeSo;
        private Vector2Int _origin;

        // Called when the object is created
        public void Init(Vector2Int origin, PlacedObjectTypeSO placedObjectTypeSo) {
            _origin = origin;
            _placedObjectTypeSo = placedObjectTypeSo;
        
            PlacedBuildingInformation.Name = placedObjectTypeSo.BaseBuildingInformation.Name;
            PlacedBuildingInformation.Cost = placedObjectTypeSo.BaseBuildingInformation.Cost;
            PlacedBuildingInformation.Description = placedObjectTypeSo.BaseBuildingInformation.Description;
            PlacedBuildingInformation.Modifiers.Income = placedObjectTypeSo.BaseBuildingInformation.Modifiers.Income;
            PlacedBuildingInformation.Modifiers.SatisfactionIncrease = placedObjectTypeSo.BaseBuildingInformation.Modifiers.SatisfactionIncrease;
            
        
            var child = GetBuildingVisual();
        
            StartCoroutine(BounceAnimation(child,0.1f, 0.1f, 0.1f));
        }
        private IEnumerator BounceAnimation(Transform child, float duration, float height, float width) {
            float elapsedTime = 0;
            Vector3 startingScale = child.localScale;

            while (elapsedTime < duration) {
                float bounceY = Mathf.Sin(elapsedTime / duration * Mathf.PI) * height;
                float bounceX = Mathf.Sin(elapsedTime / duration * Mathf.PI) * width;
                child.localScale = startingScale + new Vector3(bounceX, bounceY, 0);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            child.localScale = startingScale;
        }
        public PlacedObjectTypeSO GetData() {
            return _placedObjectTypeSo;
        }
        
        public bool IsInBaseState() { // Not Upgraded yet?
            return _currentUpgradeIndex == 0;
        }
        
        public BuildingUpgrade GetNextUpgrade() { // Returns the next upgrade
            return _placedObjectTypeSo.GetNextUpgrade(_currentUpgradeIndex);
        }
        
        public BuildingUpgrade GetCurrentUpgrade() { // Returns the current upgrade
            // Decrement the index to get the current upgrade, due to _currentUpgradeIndex being incremented after the upgrade
            return _placedObjectTypeSo.GetNextUpgrade(_currentUpgradeIndex - 1);
        }
    
        public bool CanAffordUpgrade(uint currentMoney) {
            return _placedObjectTypeSo.GetNextUpgrade(_currentUpgradeIndex).IsAffordable(currentMoney);
        }
    
        /// <summary>
        /// Delete the old building at place the upgrade in its place
        /// </summary>
        public void Upgrade() {
            // Get the next upgrade
            var nextUpgrade = _placedObjectTypeSo.GetNextUpgrade(_currentUpgradeIndex);
        
            // Set the new building information
            PlacedBuildingInformation.Name = nextUpgrade.Name;
            PlacedBuildingInformation.Cost = nextUpgrade.Cost;
            PlacedBuildingInformation.Description = nextUpgrade.Description;
            PlacedBuildingInformation.Modifiers.Income = nextUpgrade.Modifiers.Income;
            PlacedBuildingInformation.Modifiers.SatisfactionIncrease = nextUpgrade.Modifiers.SatisfactionIncrease;
        
            // New Building
            var visual = GetBuildingVisual();
            // Destroy old building
            Destroy(visual.gameObject);
        
            // Instantiate the new building
            var newVisual = Instantiate(nextUpgrade.Prefab, transform.position, Quaternion.identity, transform);
            // Bounce Animation
            StartCoroutine(BounceAnimation(newVisual,0.1f, 0.1f, 0.1f));
        
            // Increment the upgrade index
            _currentUpgradeIndex++;
        }

        /// <summary>
        /// Returns the visual of the building
        /// </summary>
        /// <returns></returns>
        private Transform GetBuildingVisual() {
            var childCount = transform.childCount;
            if(childCount > 1) {
                Debug.LogError("More than one child found in PlacedObject, should ONLY be visual, else update this method.");
            }
        
            return transform.GetChild(0);
        }
    
        public List<Vector2Int> GetGridPositionList() {
            return _placedObjectTypeSo.GetGridPositionList(_origin);
        }
        public void DestroySelf() {
            Destroy(gameObject);
        }
    }
}