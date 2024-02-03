using UnityEditor;
using UnityEngine;

namespace Util.Map.Editor
{
    [CustomEditor(typeof(MapInfoMono))]
    [CanEditMultipleObjects]
    public class MapInfoMonoEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            if (GUILayout.Button("Set Quick Info"))
            {
                foreach (var target in targets)
                {
                    var script = (MapInfoMono)target;

                    var collider = script.GetComponent<BoxCollider>();
                    if (collider != null)
                    {
                        script.info.pivot = collider.center;
                        script.info.size = collider.size;
                    }
                }
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}