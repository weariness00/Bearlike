using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using User.MagicCotton;

namespace SceneExtension
{
    [CustomEditor(typeof(MagicCottonSceneManager))]
    public class MagicCottonSceneManagerEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            // VisualElement 루트를 생성합니다.
            var root = new VisualElement();

            // 기본 인스펙터 추가
            InspectorElement.FillDefaultInspector(root, serializedObject, this);

            CottonBlockUpdate(root);
            
            return root;
        }

        private void CottonBlockUpdate(VisualElement root)
        {
            var button = new Button(() =>
            {
                var cottonList = FindObjectsByType<MagicCottonBase>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                
                foreach (var cotton in cottonList)
                {
                    var block = cotton.block;
                    
                    if(!block) continue;

                    block.SetIcon(cotton.icon);
                    block.SetMaxLevel(cotton.Level.Max);
                }
                
                Repaint();
                
                Debug.Log("모든 Cotton Block UI들을 베이스가 되는 Block에 대해 업데이트\n" +
                          "현재 업데이트 목록 [Level]");
            })
            {
                text = "Block Update"
            };

            // 버튼을 루트 요소에 추가
            root.Add(button);
        }
    }
}