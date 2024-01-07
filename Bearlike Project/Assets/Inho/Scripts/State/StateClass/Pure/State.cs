//---------------------------------------------------------------------------------
// 추상클래스 ObjectState와 인터페이스 ICondition의 합친 가장 기본적인 State Class
//---------------------------------------------------------------------------------

namespace Inho.Scripts.State
{
    public abstract class State : ObjectState, ICondition
    {
        // ICondition Interface Function
        public abstract bool On(eCondition condition);
            
        public abstract bool NormalityIsOn();
        public abstract bool PoisonedIsOn();
        public abstract bool WeakIsOn();
            
        public abstract void AddCondition(eCondition condition);
        public abstract void DelCondition(eCondition condition);
    }
}