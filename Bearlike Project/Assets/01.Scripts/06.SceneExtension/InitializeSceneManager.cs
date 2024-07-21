using Manager;
using UnityEngine.SceneManagement;

namespace SceneExtension
{
    public class InitializeSceneManager : SceneManagerExtension
    {
        public static InitializeSceneManager Instance { get; set; }

        public override void Awake()
        {
            base.Awake();

            Instance = this;

            SceneManager.LoadScene(SceneList.GetScene("Login"));
        }
    }
}

