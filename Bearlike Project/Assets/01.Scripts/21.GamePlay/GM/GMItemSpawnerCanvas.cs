using Data;
using Item;
using Photon;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.GM
{
    public class GMItemSpawnerCanvas : NetworkBehaviourEx
    {
        public Canvas canvas;
        
        public Button spawnButton;

        [Header("Monster Info UI")] 
        public GameObject infoBlockObject;
        public RawImage icon;
        public TMP_Text nameText;

        private PlayerController _player;
        private ItemBase _targetItem;

        public override void Spawned()
        {
            Object.AssignInputAuthority(Runner.LocalPlayer);
            
            _player = Runner.FindObject(UserData.Instance.UserDictionary.Get(Runner.LocalPlayer).NetworkId).GetComponent<PlayerController>();
            
            foreach (var item in ItemObjectList.Instance.GetList())
            {
                var itemInfo = ItemBase.GetInfoData(item.Id);
                
                icon.texture = item.Icon;
                nameText.text = itemInfo.name;
                var obj = Instantiate(infoBlockObject, infoBlockObject.transform.parent);
                var toggle = obj.GetComponent<Toggle>();
                
                toggle.onValueChanged.AddListener((value) =>
                {
                    if(value) OnChangeToggle(item);
                });
                obj.SetActive(true);
            }
            
            spawnButton.onClick.AddListener(OnItemSpawnButton);
        }

        private void OnItemSpawnButton()
        {
             var obj = Instantiate(_targetItem, _player.transform.position + _player.transform.forward, Quaternion.identity);
             var item = obj.GetComponent<ItemBase>();
        }

        private void OnChangeToggle(ItemBase item)
        {
            _targetItem = item;
        }
    }
}

