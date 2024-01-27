using Skill.SkillClass.FirstDoll.PureSkill;

namespace Inho.Scripts.Skill.SkillClass.FirstDoll
{
    public class FirstDoll
    {
        public Pure.Skill _filppingCoin;

        public FirstDoll()
        {
            _filppingCoin = new FlippingCoin();
        }
    }
}