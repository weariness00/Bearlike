﻿using Status;
using Util;

namespace User
{
    public class UserInformation : Singleton<UserInformation>
    {
        public StatusValue<int> cottonCoin = new StatusValue<int>(); // 게임이 끝나도 다음에 사용할 수 있는 보상
    }
}