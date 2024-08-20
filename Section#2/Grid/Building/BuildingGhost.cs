﻿using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Grid.Building {
    public class BuildingGhost : MonoBehaviour {
        [EnumToggleButtons][SerializeField] private GhostVisualType ghostVisualType;
        // We are acccessing the events of the GridBuildingSystem
        [SerializeField] private GridBuildingSystem gridBuildingSystem;
    
        // Layer of the Ghost and it Childs, can not be LayerMask, multiple layers are not supported
        [SerializeField] [ValueDropdown("GetLayers")] private int selectedLayer;
    
        // Current Visual Ghost
        private Transform _visual;
    
        // Ghost renderer to set the color
        private Tilemap _ghostRenderer;
        private SpriteRenderer _spriteRenderer;
        private Color _canBuildColor;

        private void Start() {
            gridBuildingSystem.OnSelectChanged += OnBuildingSystemInstanceSelectChanged;
            gridBuildingSystem.OnCanBuildChanged += OnCanBuildChanged;
            gridBuildingSystem.OnStoppedBuilding += DespawnGhost;
        }

        private void OnDestroy() {
            gridBuildingSystem.OnSelectChanged -= OnBuildingSystemInstanceSelectChanged;
            gridBuildingSystem.OnCanBuildChanged -= OnCanBuildChanged;
            gridBuildingSystem.OnStoppedBuilding -= DespawnGhost;
        }

        /// <summary>
        /// Switches the Color of the Ghost
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCanBuildChanged(object sender, GridBuildingSystem.CanBuildChangedEventArgs e) {
            //TODO: Currently this is called from GridBuildingSystem, also from when Selling and this calls DespawnGhost via event, 
            //TODO: But because onvalue of DebugMoney changed this method is called, it tries to access the _visual which is null
            if(_visual == null) return;
            _canBuildColor = e.CanBuild ? Color.green : Color.red;
            
            
            var colorSetter = ghostVisualType switch {
                GhostVisualType.SpriteRenderer => (color => _spriteRenderer.color = color),
                GhostVisualType.Tilemap => new Action<Color>(color => _ghostRenderer.color = color),
                _ => throw new ArgumentOutOfRangeException()
            };

            colorSetter(_canBuildColor);
        }
    
        /// <summary>
        /// Destroys the old Ghost
        /// </summary>
        private void DespawnGhost() {
            if(_visual != null) {
                Destroy(_visual.gameObject);
                _visual = null;
            }
        }

        /// <summary>
        /// Despawns the old ghost and spawns a new one with the new position and the new PlacedObjectTypeSO
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="placedObjectTypeSo"></param>
        private void OnBuildingSystemInstanceSelectChanged(Vector3 targetPosition, PlacedObjectTypeSO placedObjectTypeSo) {
            // SO should never be null
            if (!placedObjectTypeSo) {
                Debug.LogError("PlacedObjectTypeSO is null");
                return;
            }
        
            // Destroy the old Ghost
            DespawnGhost();
        
            // Spawn the new Ghost in the correct position with the correct color
            _visual = Instantiate(placedObjectTypeSo.BaseBuildingInformation.Prefab, targetPosition, Quaternion.identity);
            
            var componentSetter = ghostVisualType switch {
                GhostVisualType.SpriteRenderer => new Action(() =>{
                    _spriteRenderer = _visual.GetComponentInChildren<SpriteRenderer>();
                    _spriteRenderer.color = _canBuildColor;
                }),
                GhostVisualType.Tilemap => new Action(() => {
                    _ghostRenderer = _visual.GetComponent<Tilemap>();
                    _ghostRenderer.color = _canBuildColor;
                }),
                _ => throw new ArgumentOutOfRangeException()
            };

            componentSetter();
            _visual.parent = transform;
        
            // Set the Layer of the Ghost
            SetLayerRecursively(_visual.gameObject, selectedLayer);
        }

        /// <summary>
        /// Recursive Layer setting foreach child of the passed GO
        /// </summary>
        /// <param name="visualGameObject"></param>
        /// <param name="layer"></param>
        private void SetLayerRecursively(GameObject visualGameObject, int layer) {
            foreach (Transform child in visualGameObject.transform) {
                SetLayerRecursively(child.gameObject, layer);
            }
            visualGameObject.layer = layer;
        }
        /// <summary>
        /// Returns a Dropdown List of all Layers
        /// </summary>
        /// <returns></returns>
        private static ValueDropdownList<int> GetLayers() {
            ValueDropdownList<int> layers = new ValueDropdownList<int>();
            for (int i = 0; i < 32; i++) {
                string layerName = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(layerName)) {
                    layers.Add(layerName, i);
                }
            }
            return layers;
        }
        
        public enum GhostVisualType {
            Tilemap,
            SpriteRenderer
        }
    }
}