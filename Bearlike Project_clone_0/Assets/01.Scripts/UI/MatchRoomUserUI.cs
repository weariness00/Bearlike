using System;
using System.Collections.Generic;
using Fusion;
using GamePlay.StageLevel;
using Script.Data;
using Script.Manager;
using Script.Photon;
using TMPro;
using UnityEngine;

public class MatchRoomUserUI : MonoBehaviour
{
    public TMP_Text[] users_Text;

    public void UpdateData(UserDataStruct[] dataList)
    {
        for (int i = 0; i < NetworkProjectConfig.Global.Simulation.PlayerCount && i < dataList.Length; i++)
        {
            users_Text[i].text = dataList[i].Name.ToString();
        }
    }
}