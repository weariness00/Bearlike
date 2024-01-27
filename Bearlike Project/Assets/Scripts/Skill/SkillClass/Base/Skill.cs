namespace Inho.Scripts.Skill.SkillClass.Base
{
    public abstract class Skill
    {
        // Member Variable
        protected TimeValue.TimeValue mDuration;
        protected TimeValue.TimeValue mCoolTime;
        
        // Member Function
        public float GetDration() { return mDuration.current;}
        public float GetCoolTime() { return mCoolTime.current;}

        public abstract void Run();
    }
}