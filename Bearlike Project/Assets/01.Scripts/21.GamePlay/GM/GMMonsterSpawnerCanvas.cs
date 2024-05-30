using System.Collections.Generic;
using Data;
using Monster;
using Photon;
using Player;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.GM
{
    public class GMMonsterSpawnerCanvas : NetworkBehaviourEx
    {
        public Canvas canvas;
        public List<MonsterBase> monsterList = new List<MonsterBase>();

        public Button spawnButton;

        [Header("Monster Info UI")] 
        public GameObject infoBlockObject;
        public Image icon;
        public TMP_Text nameText;

        private MonsterBase _spawnTargetMonster;
        private PlayerController player;

        public override void Spawned()
        {
            Object.AssignInputAuthority(Runner.LocalPlayer);
            
            player = Runner.FindObject(UserData.Instance.UserDictionary.Get(Runner.LocalPlayer).NetworkId).GetComponent<PlayerController>();
            
            foreach (var monsterBase in monsterList)
            {
                nameText.text = monsterBase.name;
                var block = Instantiate(infoBlockObject, infoBlockObject.transform.parent);
                var toggle = block.GetComponent<Toggle>();
                toggle.onValueChanged.AddListener((value) =>
                {
                    if (value) OnChangeToggle(monsterBase);
                });
                block.SetActive(true);
            }
            spawnButton.onClick.AddListener(OnSpawnMonster);
        }

        private void OnSpawnMonster()
        {
            Runner.SpawnAsync(_spawnTargetMonster.gameObject, player.transform.position + player.transform.forward);
        }

        private void OnChangeToggle(MonsterBase monster)
        {
            _spawnTargetMonster = monster;
        }
    }
}

