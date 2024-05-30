using System;
using Fusion;
using Photon;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SessionBlockHandle : MonoBehaviour
    {
        public TMP_Text roomNameText;
        public TMP_Text playerCountText;
        public Button joinButton;

        private void Start()
        {
            joinButton.onClick.AddListener(JoinSession);
        }

        public void SetSessionInfo(SessionInfo info)
        {
            roomNameText.text = info.Name;
            playerCountText.text = $"{info.PlayerCount} / {info.MaxPlayers}";
        }

        public async void JoinSession()
        {
            await NetworkManager.Instance.JoinRoom(roomNameText.text);
        }
    }
}