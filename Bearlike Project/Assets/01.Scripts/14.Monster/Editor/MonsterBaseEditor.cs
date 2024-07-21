using GamePlay.DeadBodyObstacle;
using Monster;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Scripts.Monster
{
    [CustomEditor(typeof(MonsterBase), true)]
    public class MonsterBaseEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            // VisualElement 루트를 생성합니다.
            MonsterBase script = target as MonsterBase;
            var root = new VisualElement();

            // 기본 인스펙터 추가
            InspectorElement.FillDefaultInspector(root, serializedObject, this);

            MakeDeadBodyButton(root, script);
            
            return root;
        }

        private void MakeDeadBodyButton(VisualElement root, MonsterBase script)
        {
            if (script.TryGetComponent(out DeadBodyObstacleObject component)) return;

            var button = new Button(() =>
            {
                Debug.Log("Dead Body 컴포넌트 생성");
                script.AddComponent<DeadBodyObstacleObject>();
            })
            {
                text = "Generate DeadBodyObstacleObject Component"
            };

            // 버튼을 루트 요소에 추가
            root.Add(button);
        }
    }
}