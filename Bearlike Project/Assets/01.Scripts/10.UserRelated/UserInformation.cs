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
            var userData = FireBaseDataBaseManager.RootReference.GetChild($"UserData/{FireBaseAuthManager.UserId}");
            userData.SnapShot(snapshot =>
            {
                if(snapshot.HasChild("Name") == false)
                    snapshot.Reference.SetChild("Name", "Unknown");
                
                if (snapshot.HasChild("CottonCoin"))
                {
                    _cottonCoin.Current = snapshot.Child("CottonCoin").ValueInt();
                }
                else
                {
                    snapshot.Reference.SetChild("CottonCoin", 0);
                }

                if (snapshot.HasChild("MagicCottonContainer") == false)
                    snapshot.Reference.SetChild("MagicCottonContainer", true);
            });
        }

        public int GetCoin() => _cottonCoin.Current;
        public void SetCoin(int value)
        {
            _cottonCoin.Current = value;
            FireBaseDataBaseManager.RootReference.GetChild($"UserData/{FireBaseAuthManager.UserId}").SnapShot(snapshot =>
            {
                snapshot.Child("CottonCoin").Reference.SetValueAsync(value);
            });
        }
        public void AddCoin(int value) => SetCoin(_cottonCoin.Current + value);
    }
}