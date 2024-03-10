using TMPro;
using UnityEngine;

namespace Manager
{
    public class LobbyManager : MonoBehaviour
    {
        public AudioClip BGM;
        public TMP_InputField userID;

        private void Start()
        {
            SoundManager.Play(BGM, SoundManager.SoundType.BGM);
        }
    }
}