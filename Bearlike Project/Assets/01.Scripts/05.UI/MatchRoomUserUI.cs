using System;
using Data;
using Fusion;
using Photon;
using Script.Util;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatchRoomUserUI : NetworkBehaviour
{
    public TMP_Text[] users_Text;
    public Button exitButton;

    private void Awake()
    {
        exitButton.onClick.AddListener(OnExit);
        UserData.Instance.UserJoinAction += UserActionToDataUpdate;
        UserData.Instance.UserLeftAction += UserActionToDataUpdate;
    }

    private void Update()
    {   
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnExit();
        }
    }

    private void OnDestroy()
    {
        UserData.Instance.UserJoinAction -= UserActionToDataUpdate;
        UserData.Instance.UserLeftAction -= UserActionToDataUpdate;
    }

    public override void Spawned()
    {
        DataUpdateRPC();
    }

    // UserData의 Action들에 넣고 뺼 용으로 사용하는 함수
    private void UserActionToDataUpdate(PlayerRef playerRef) => DataUpdateRPC();
    [Rpc(RpcSources.All,RpcTargets.All)]
    public void DataUpdateRPC() => DataUpdate();
    public void DataUpdate()
    {
        var items = NetworkUtil.DictionaryItems(UserData.Instance.UserDictionary);
        UpdateData(items);
    }
    
    public void UpdateData(UserDataStruct[] dataList)
    {
        for (int i = 0; i < 3; i++)
        {
            users_Text[i].text = "Unknown";
        }

        for (int i = 0; i < dataList.Length; i++)
        {
            users_Text[i].text = dataList[i].Name.ToString();
        }
    }

    public void OnExit()
    {
        NetworkManager.Runner.Shutdown();
    }
}