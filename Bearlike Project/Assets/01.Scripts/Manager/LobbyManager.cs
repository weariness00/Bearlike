using System;
using TMPro;
using UnityEngine;


public class LobbyManager : MonoBehaviour
{
    public AudioClip BGM;
    public TMP_InputField userID;

    private void Start()
    {
        SoundManager.Play(BGM, SoundManager.SoundType.BGM);
    }
}