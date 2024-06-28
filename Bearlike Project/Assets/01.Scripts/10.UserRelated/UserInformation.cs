using System.Collections.Generic;
using Manager.FireBase;
using Status;
using Util;

namespace User
{
    public class UserInformation : Singleton<UserInformation>
    {
        public CottonInfo cottonInfo;

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
            cottonInfo.Init();
        }
    }

    [System.Serializable]
    public class CottonInfo
    {
        private StatusValue<int> _cottonCoin = new StatusValue<int>(){Max = 99999}; // 게임이 끝나도 다음에 사용할 수 있는 보상

        public void Init()
        {
            FireBaseDataBaseManager.ReadRealTimeDataBase($"UserData/{FireBaseAuthManager.UserId}", snapshot =>
            {
                if (snapshot.HasChild("CottonCoin"))
                {
                    _cottonCoin.Current = snapshot.Child("CottonCoin").Value();
                }
                else
                {
                    FireBaseDataBaseManager.WriteRealTimeDataBase($"UserData/{FireBaseAuthManager.UserId}", new Dictionary<string, object>(){{"CottonCoin", 0}});
                }
            });
        }

        public int GetCoin() => _cottonCoin.Current;
        public void SetCoin(int value)
        {
            _cottonCoin.Current = value;
            FireBaseDataBaseManager.WriteRealTimeDataBase($"UserData/{FireBaseAuthManager.UserId}", new Dictionary<string, object>(){{"CottonCoin", value}});
        }
    }
}