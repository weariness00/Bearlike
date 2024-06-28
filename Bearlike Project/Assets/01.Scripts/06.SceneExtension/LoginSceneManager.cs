using System;
using Manager;
using User;

namespace SceneExtension
{
    public class LoginSceneManager : SceneManagerExtension
    {
        private void Start()
        {
            UserInformation.Destroy();
        }
    }
}

