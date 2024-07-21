using System.Collections.Generic;
using System.IO;
using Script.Data;
using UnityEngine;
using UnityEngine.SceneManagement;
using Util;

namespace SceneExtension
{
    [DefaultExecutionOrder((int)DefaultExecutionOrderType.Inisialize)]
    public class SceneList : Singleton<SceneList>
    {
        public static SceneReference GetScene(string sceneName) => Instance.GetSceneByName(sceneName);
        public static SceneReference GetScene(int index) => Instance.GetSceneByIndex(index);
        
        [SerializeField] private List<SceneReference> sceneReferencesList;
        
        public SceneReference GetSceneByName(string sceneName)
        {
            foreach (var sceneReference in sceneReferencesList)
            {
                if (sceneName == Path.GetFileNameWithoutExtension(sceneReference))
                {
                    return sceneReference;
                }
            }

            return null;
        }

        public SceneReference GetSceneByIndex(int index)
        {
            foreach (var sceneReference in sceneReferencesList)
            {
                if (index == SceneUtility.GetBuildIndexByScenePath(sceneReference))
                {
                    return sceneReference;
                }
            }

            return null;
        }
    }
}

