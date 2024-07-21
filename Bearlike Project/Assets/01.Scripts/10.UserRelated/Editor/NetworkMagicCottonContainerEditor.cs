using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using User.MagicCotton;
using UserRelated.MagicCotton;

namespace Scripts.UserRelated
{
    [CustomEditor(typeof(NetworkMagicCottonContainer))]
    public class NetworkMagicCottonContainerEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            // VisualElement 루트를 생성합니다.
            var root = new VisualElement();

            // 기본 인스펙터 추가
            InspectorElement.FillDefaultInspector(root, serializedObject, this);

            ListUpdate(root);
            
            return root;
        }

        private void ListUpdate(VisualElement root)
        {
            var button = new Button(() =>
            {
                var script = target as NetworkMagicCottonContainer;
                
                var list = MagicCottonList.Instance.GetList();
                var mcDict = new Dictionary<int, MagicCottonBase>();
                // 현재 mc list에 있는 것들을 id로 분별
                foreach (var mc in list)
                    mcDict.Add(mc.Id, mc);

                // network Magic Cotton Container에 있는 MC 들은 제외
                var netList = script.GetList();
                foreach (var mc in netList)
                    mcDict.Remove(mc.Id);
                
                // 없는 것만 생성
                foreach (var (id, mc) in mcDict)
                   Instantiate(mc.gameObject, script.transform);
                
                script.SetList(script.GetComponentsInChildren<MagicCottonBase>().ToList());
                
                EditorUtility.SetDirty(target);

                Debug.Log("Network Magic Cotton Container 초기화");
            })
            {
                text = "List Update"
            };
            
            root.Add(button);
        }
    }
}

