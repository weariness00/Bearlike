using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Util;

namespace Skill.Editor
{
    [CustomEditor(typeof(SkillObjectList))]
    public class SkillObjectListEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("스킬 리스트 불러오기"))
            {
                var script = target as SkillObjectList;

                var guids = AssetDatabase.FindAssets("t:GameObject", new[] { "Assets/02.Prefabs/12.Skill" });
                List<SkillBase> skillList = new List<SkillBase>();
                
                foreach (string guid in guids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    GameObject gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    if (gameObject.TryGetComponent(out SkillBase skill))
                        skillList.Add(skill);
                }

                JsonConvertExtension.Load("Skill", (json) =>
                {
                    var data = JsonConvert.DeserializeObject<SkillJsonData[]>(json);
                    
                    foreach (var skillBase in skillList)
                    {
                        skillBase.SetJsonData( data.FirstOrDefault(s => s.ID == skillBase.id));
                    }
                });

                script.skillList = skillList;
            }
        }
    }
}