namespace State.StateClass.Pure
{
    /// <summary>
    /// 추상클래스 ObjectState와 인터페이스 ICondition의 합친 가장 기본적인 State Class
    /// </summary>
    public abstract class State : ObjectState, ICondition
    {
        // ICondition Interface Function
        public abstract bool On(int condition);
            
        public abstract bool NormalityIsOn();
        public abstract bool PoisonedIsOn();
        public abstract bool WeakIsOn();
            
        public abstract void AddCondition(int condition);
        public abstract void DelCondition(int condition);
    }
}