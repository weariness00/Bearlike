using System;
using System.Collections;
using Data;
using DG.Tweening;
using Fusion;
using GamePlay.UI;
using Manager;
using Photon;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using User;
using Util;

namespace GamePlay
{
    public class GameResult : MonoBehaviour
    {
        public Button lobbyButton;

        public Image backgroundImage;
        public Image gameClearImage;
        [SerializeField] private TMP_Text timeText;
        public Image gameOverImage;

        [SerializeField] private GameObject playerResultObject;
        [SerializeField] private GameObject playerResultBlockPrefab;
        
        private void Start()
        {
            Cursor.lockState = CursorLockMode.Confined;
            
            backgroundImage.DOFade(1f, 2f); 
            
            if(GameManager.Instance.isGameClear)
                OnGameClear();
            else if (GameManager.Instance.isGameOver)
                OnGameOver();

            Compensation();
            
            foreach (var player in FindObjectsOfType<PlayerController>())
            {
                var obj = Instantiate(playerResultBlockPrefab, playerResultBlockPrefab.transform.parent);
                var block = obj.GetComponent<PlayerResultBlock>();
                
                block.gameObject.SetActive(true);
                block.SetPlayerData(player);
            }
        }

        void ButtonInit()
        {
            lobbyButton.onClick.AddListener(() => NetworkManager.Runner.Shutdown());
            
            var lobbyText = lobbyButton.GetComponentInChildren<TMP_Text>();
            {
                var color = lobbyText.color;
                color.a = 0;
                lobbyText.color = color;
            }
            {
                var color = lobbyButton.image.color;
                color.a = 0;
                lobbyButton.image.color = color;
            }
            
            lobbyText.DOFade(1, 1).SetDelay(2f);
            lobbyButton.image.DOFade(1, 1).SetDelay(2f);
            
            lobbyButton.gameObject.SetActive(true);
        }

        public void OnGameClear()
        {
            StartCoroutine(OnGameClearCoroutine());
        }

        private IEnumerator OnGameClearCoroutine()
        {
            yield return new WaitForSeconds(2f);
            
            gameClearImage.gameObject.SetActive(true);
            gameClearImage.rectTransform.DOPunchScale(Vector3.one, 1f);

            yield return new WaitForSeconds(1f);
            
            InitPlayerResult();
            ButtonInit();
        }

        public void OnGameOver()
        {
            ButtonInit();
            
            gameOverImage.gameObject.SetActive(true);
            
            gameOverImage.DOFade(0, 0);
            gameOverImage.rectTransform.DOShakeScale(1.5f, 0.3f);
            gameOverImage.DOFade(1f, 1f).SetDelay(0.5f);
        }

        // 게임 결과의 보상 지급
        private void Compensation()
        {
            PlayerController inputPlayer = null;
            var pcs = FindObjectsOfType<PlayerController>();
            foreach (var pc in pcs)
            {
                if (pc.HasInputAuthority)
                {
                    inputPlayer = pc;
                    break;
                }
            }

            if (inputPlayer == null)
            {
                DebugManager.LogError("보상을 지급할 플레이어를 찾을 수 없습니다.");
                return;
            }
            
            // 레벨에 따른 Cotton Coin 지급
            int level = inputPlayer.status.level.Current;
            int count = 1;
            while (level >= 10)
            {
                level -= 10;
                count++;
            }
            
            var cottonCoin = inputPlayer.status.level.Current * count;
            UserInformation.Instance.cottonInfo.AddCoin(cottonCoin);
        }

        private void InitPlayerResult()
        {
            playerResultObject.SetActive(true);
            timeText.text = GameManager.Instance.PlayTimer.TimeString();
        }
    }
}

