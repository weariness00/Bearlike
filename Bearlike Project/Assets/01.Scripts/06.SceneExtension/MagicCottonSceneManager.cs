using System;
using Manager;
using Script.Data;
using UnityEngine;

namespace SceneExtension
{
    [DefaultExecutionOrder((int)DefaultExecutionOrderType.Inisialize)]
    public class MagicCottonSceneManager : SceneManagerExtension
    {
        public Transform blockTransform;
        public GameObject blockObject;

        private void Start()
        {
            var lobbyManager = FindObjectOfType<LobbyManager>();
            if (lobbyManager)
            {
                lobbyManager.magicCottonSceneButton.onClick.AddListener(() =>
                {
                    foreach (var o in activeFalseObjectList)
                    {
                        o.SetActive(true);
                    }
                });
            }
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                foreach (var o in activeFalseObjectList)
                {
                    o.SetActive(false);
                }
            }
        }
        
        
    }
}

