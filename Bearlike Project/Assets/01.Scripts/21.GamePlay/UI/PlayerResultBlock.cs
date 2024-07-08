using System.Linq;
using Fusion;
using Photon;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace GamePlay.UI
{
    public class PlayerResultBlock : NetworkBehaviourEx
    {
        [Networked] public NetworkId PlayerId { get; set; }
        [Networked] public NetworkId ParentId { get; set; }
        
        [SerializeField] private Image playerImage;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text hasSkillCountText;
        [SerializeField] private TMP_Text maxLevelSkillCountText;
        [SerializeField] private TMP_Text monsterKillCountText;
        [SerializeField] private TMP_Text damageCountText;

        public override void Spawned()
        {
            base.Spawned();

            var parentObj = Runner.FindObject(ParentId);
            if (parentObj)
            {
                transform.parent = parentObj.transform;
            }
            
            var playerObj = Runner.FindObject(PlayerId);
            if (playerObj)
            {
                var player = playerObj.GetComponent<PlayerController>();
                var playerProgress = FindObjectsOfType<PlayerProgressBlock>(true).First(p => p.Object.InputAuthority == player.Object.InputAuthority);

                playerImage.sprite = player.icon;
                levelText.text = player.status.level.Current.ToString();
                nameText.text = playerProgress.GetPlayerName();
                hasSkillCountText.text = player.skillSystem.SkillLength.ToString();
                maxLevelSkillCountText.text = player.skillSystem.GetMaxLevelSkillCount().ToString();
                monsterKillCountText.text = playerProgress.KillCount.ToString();
                damageCountText.text = playerProgress.DamageCount.ToString();
            }
        }
    }
}

