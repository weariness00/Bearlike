using Manager;
using Photon;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace UI
{
    public class GameSettingUI : Singleton<GameSettingUI>
    {
        #region Static

        private UniqueQueue<GameObject> _ActiveUIQueue;
        public static void AddActiveUI(GameObject uiObject) => Instance._ActiveUIQueue.Enqueue(uiObject);

        #endregion
        
        public Button goLobbyButton;
        public Button quitGameButton;

        private bool childrenActiveSelf;

        private void Awake()
        {
            base.Awake();
            _ActiveUIQueue = new UniqueQueue<GameObject>();
        }

        private void Start()
        {
            goLobbyButton.onClick.AddListener(() => NetworkManager.Runner.Shutdown());
#if UNITY_EDITOR
            quitGameButton.onClick.AddListener(() => UnityEditor.EditorApplication.isPlaying = false);
#else
            quitGameButton.onClick.AddListener(Application.Quit);
#endif
            ActiveChildren(false);
        }

        public void Update()
        {
            if (KeyManager.InputActionDown(KeyToAction.Esc))
            {
                if (_ActiveUIQueue.IsEmpty())
                {
                    ActiveChildren(!childrenActiveSelf);
                }
                else
                {
                    var queue = _ActiveUIQueue.Dequeue();
                    queue.SetActive(false);
                }
            }
        }

        void ActiveChildren(bool value)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(value);
            }

            childrenActiveSelf = value;
        }

        public static void ActiveUIAllDisable()
        {
            var list = Instance._ActiveUIQueue.AllDequeue();
            foreach (var uiObj in list)
                uiObj.SetActive(false);
        }
    }
}