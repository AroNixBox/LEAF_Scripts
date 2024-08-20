using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Data.Tutorial {
    [CreateAssetMenu(fileName = "TutorialInfo", menuName = "TurnBased/TutorialInfo")]
    public class TutorialInfo : ScriptableObject {
        [BoxGroup("References")] public GameObject rightScreenSpaceTutorialHint;
        [BoxGroup("References")] public GameObject rightWorldSpaceTutorialHint;
        [BoxGroup("References")] public GameObject leftScreenSpaceTutorialHint;
        [BoxGroup("References")] public GameObject leftWorldSpaceTutorialHint;
        
        public List<TutorialInfoData> tutorialData;
        
        public TutorialInfoData CreateTutorialHint(int hintIndex) {
            // Get the TutorialInfoData for the given index
            var tutorialInfoData = tutorialData[hintIndex];
            
            // Choose the correct allignment Prefab
            var screenSpacedTutorialHint = tutorialInfoData.textOrientation == TutorialInfoData.TextOrientation.Left
                ? leftScreenSpaceTutorialHint
                : rightScreenSpaceTutorialHint;
            
            var worldSpacedTutorialHint = tutorialInfoData.textOrientation == TutorialInfoData.TextOrientation.Left
                ? leftWorldSpaceTutorialHint
                : rightWorldSpaceTutorialHint;

            // Choose the correct prefab based on the hintType
            GameObject hintPrefab = tutorialInfoData.hintType switch {
                TutorialInfoData.TutorialHintType.ScreenSpace => screenSpacedTutorialHint,
                TutorialInfoData.TutorialHintType.WorldSpace_Build 
                    or TutorialInfoData.TutorialHintType.WorldSpace_Upgrade
                    or TutorialInfoData.TutorialHintType.WorldSpace_Raw => worldSpacedTutorialHint,
                _ => throw new System.ArgumentOutOfRangeException() // This should never happen
            };

            tutorialInfoData.hintObject = hintPrefab;
            return tutorialInfoData;
        }
        
        [System.Serializable]
        public class TutorialInfoData { // TODO: If Problems in Build, Rework setting hintObject in Runtime
            public GameObject hintObject { get; set; }
            [TextArea(3,10)]
            [Header("Text")]
            public string hintText;
            [EnumToggleButtons] public TextOrientation textOrientation = TextOrientation.Right; // Right is default
            [Header("Parenting")]
            public Vector2 anchorPosition;
            [Space(20)]
            [InfoBox("This Selection decides the parent of the Tutorial Hint")]
            [EnumToggleButtons] public TutorialHintType hintType;
            
            public enum TutorialHintType {
                ScreenSpace,
                WorldSpace_Build,
                WorldSpace_Upgrade,
                WorldSpace_Raw
            }
            public enum TextOrientation {
                Left,
                Right
            }
        }
    }
}
