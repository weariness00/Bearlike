using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using User.MagicCotton;

namespace User
{
    [CustomEditor(typeof(MagicCottonList))]
    public class MagicCottonListEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            // VisualElement 루트를 생성합니다.
            var root = new VisualElement();

            // 기본 인스펙터 추가
            InspectorElement.FillDefaultInspector(root, serializedObject, this);

            InitList(root);
            
            return root;
        }

        private void InitList(VisualElement root)
        {
            MagicCottonList script = target as MagicCottonList;
            
            var button = new Button(() =>
            {
                Debug.Log("Magic Cotton List 초기화");
                script.SetList(script.GetComponentsInChildren<MagicCottonBase>().ToList());
            })
            {
                text = "Initialize List"
            };

            // 버튼을 루트 요소에 추가
            root.Add(button);
        }
    }
}