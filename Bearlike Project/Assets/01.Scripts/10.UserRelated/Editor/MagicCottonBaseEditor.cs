using System.Linq;
using SceneExtension;
using UI.User;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using User.MagicCotton;

namespace User
{
    [CustomEditor(typeof(MagicCottonBase), true)]
    public class MagicCottonBaseEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            // VisualElement 루트를 생성합니다.
            var root = new VisualElement();

            // 기본 인스펙터 추가
            InspectorElement.FillDefaultInspector(root, serializedObject, this);

            MakeBlock(root);
            IconUpdate(root);
            
            return root;
        }

        private void MakeBlock(VisualElement root)
        {
            MagicCottonBase script = target as MagicCottonBase;

            if(script.block) return;
            
            var button = new Button(() =>
            {
                var mcScene = FindObjectOfType<MagicCottonSceneManager>();
                if (!mcScene)
                {
                    Debug.LogError("MagicCottonSceneManager가 존재하지 않습니다.");
                    return;
                }

                if (!mcScene.blockObject || !mcScene.blockTransform)
                {
                    Debug.LogError("MagicCottonSceneManager컴포넌트에서 block이 존재하지 않습니다.");
                    return;
                }

                var o = Instantiate(mcScene.blockObject, mcScene.blockTransform);
                var block = o.GetComponent<CottonBlock>();

                o.name = script.Name + " Block";
                o.SetActive(true);
                
                block.SetMagicCotton(script);

                script.block = block;

                Repaint();
                
                Debug.Log("Magic Cotton Block UI 생성");
            })
            {
                text = "Make Cotton Block"
            };

            // 버튼을 루트 요소에 추가
            root.Add(button);
        }

        private void IconUpdate(VisualElement root)
        {
            MagicCottonBase script = target as MagicCottonBase;

            if(!script.block) return;
            
            var button = new Button(() =>
            {
                script.block.icon.sprite = script.icon;
                Repaint();
                
                Debug.Log($"{script.Name}의 Icon을 업데이트");
            })
            {
                text = "Block Icon Update"
            };

            // 버튼을 루트 요소에 추가
            root.Add(button);
        }
    }
}