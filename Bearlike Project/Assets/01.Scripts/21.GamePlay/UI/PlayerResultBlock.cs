using System;
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
    public class PlayerResultBlock : MonoBehaviour
    {
        [SerializeField] private Image playerImage;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text hasSkillCountText;
        [SerializeField] private TMP_Text maxLevelSkillCountText;
        [SerializeField] private TMP_Text monsterKillCountText;
        [SerializeField] private TMP_Text damageCountText;

        public void SetPlayerData(PlayerController player)
        {
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

