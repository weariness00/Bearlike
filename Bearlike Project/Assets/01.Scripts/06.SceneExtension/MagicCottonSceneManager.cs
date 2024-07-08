using System;
using Manager;
using Manager.FireBase;
using Script.Data;
using TMPro;
using UnityEngine;

namespace SceneExtension
{
    [DefaultExecutionOrder((int)DefaultExecutionOrderType.Inisialize)]
    public class MagicCottonSceneManager : SceneManagerExtension
    {
        public Transform blockTransform;
        public GameObject blockObject;

        [Header("Canvas")] 
        [SerializeField] private TMP_Text cottonCoinText; 

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
                    UIManager.AddActiveUI(activeFalseObjectList.ToArray());
                });
            }

            CottonCoinInit();
        }

        private void CottonCoinInit()
        {
            var reference = FireBaseDataBaseManager.RootReference.GetChild($"UserData/{FireBaseAuthManager.UserId}/CottonCoin");
            reference.SnapShot(snapshot =>
            {
                cottonCoinText.text = snapshot.ValueString();
            });
            
            reference.ValueChanged += (sender, args) =>
            {
                if (args == null)
                {
                    Debug.LogError("ValueChangedEventArgs is null");
                    return;
                }

                if (args.DatabaseError != null)
                {
                    Debug.LogError(args.DatabaseError.Message);
                    return;
                }
                
                cottonCoinText.text = args.Snapshot.ValueString();
            };
        }
    }
}

