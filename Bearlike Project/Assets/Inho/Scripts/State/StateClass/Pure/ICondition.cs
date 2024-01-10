//
// Object의 상태를 나타내는 인터페이스 &lt;
//

namespace Inho.Scripts.State
{
    public enum eCondition
    {
        Normality = 0b_0000_0000, // 정상
        Poisoned = 0b_0000_0001, // 중독
        Weak = 0b_0000_0010, // 취약 => 최종 데미지 1.5배 증가
        Count
    }
    
    public interface ICondition
    {
        // Condition Determination Function
        public bool On(eCondition condition);
        
        public bool NormalityIsOn();
        public bool PoisonedIsOn();
        public bool WeakIsOn();
        
        public void AddCondition(eCondition condition);
        public void DelCondition(eCondition condition);
    }
}