using Player;
using Status;
using UnityEngine;

namespace Item.Container
{
    /// <summary>
    /// 플레이어의 스킬 쿨타임을 일정 비율로 줄여주는 아이템
    /// ex ) 쿨타임 100초 x 감소 비율 10% = 쿨타임 90초
    /// </summary>
    public class ItemSkillCoolTimeDecrease : ItemBase
    {
        public StatusValue<float> decreasePercentage = new StatusValue<float>();

    }
}

