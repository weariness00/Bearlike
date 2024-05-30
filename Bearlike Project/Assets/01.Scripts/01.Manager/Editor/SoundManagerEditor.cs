using UnityEditor;
using UnityEngine;

namespace Script.Manager.Editor
{
    [CustomEditor(typeof(SoundManager))]
    public class SoundManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var soundManager = (SoundManager)target;

            if (GUILayout.Button("Generate Audio Sources"))
            {
                soundManager.AudioSourcesGenerate();
            }
        }
    }
}