using Util;

namespace Manager.FireBase
{
    public class FireBaseManager : Singleton<FireBaseManager>
    {
        void OnApplicationQuit()
        {
            FireBaseAuthManager.LogOut();
        }
    }
}

