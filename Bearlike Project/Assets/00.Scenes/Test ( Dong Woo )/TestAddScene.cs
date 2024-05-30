
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Test
{
    public class TestAddScene : NetworkBehaviour
    {
        public SceneReference addScene;

        public override void FixedUpdateNetwork()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                Runner.LoadScene(SceneRef.FromIndex(SceneUtility.GetBuildIndexByScenePath(addScene)), LoadSceneMode.Additive);
                Destroy(gameObject);
            }
        }
    }
}
