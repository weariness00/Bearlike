using Skill.SkillClass.FirstDoll.UniqueSkill;

namespace Skill.SkillClass.FirstDoll
{
    public class FirstDoll
    {
        public Inho.Scripts.Skill.SkillClass.Base.Skill _filppingCoin;

        public FirstDoll()
        {
            _filppingCoin = new FlippingCoin();
        }
    }
}