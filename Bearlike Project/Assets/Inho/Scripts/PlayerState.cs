namespace Inho.Scripts
{
    public class PlayerState : State, ICondition
    {
        // Member Variable
        
        // Member Function
        // State abstract class Function
        public override void Initialization()
        {
            mHP = 100;
            mForce = 10.0f;
            mCondition = eCondition.Normality;
        }
        
        public override void HealingHP()
        {
            mHP += 10.0f;
        }
        
        // ICondition Interface Function
        public bool On(eCondition condition) { return (mCondition & eCondition.Normality) == eCondition.Normality; }

        public bool NormalityIsOn() { return On(eCondition.Normality); }
        public bool PoisonedIsOn() { return On(eCondition.Poisoned); }
        public bool WeakIsOn() { return On(eCondition.Weak); }
        
        public void AddCondition(eCondition condition)
        {
            mCondition |= condition;
        }
    }
}