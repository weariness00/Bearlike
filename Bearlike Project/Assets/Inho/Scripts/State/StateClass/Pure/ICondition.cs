using System;

namespace Inho.Scripts.State
{
    /// <summary>
    /// Object의 상태를 나타내는 열거형
    /// </summary>
    public enum eCondition
    {
        Normality = 0b_0000_0000, // 정상
        Poisoned = 0b_0000_0001, // 중독
        Weak = 0b_0000_0010, // 취약 => 최종 데미지 1.5배 증가
        Count
    }
    
    /// <summary>
    /// Object의 상태를 나타내는 인터페이스
    /// </summary>
    public interface ICondition
    {
        // C#에서 가능 
        // public int x { get; set; }

        // Condition Determination Function
        public bool On(int condition);
        
        public bool NormalityIsOn();
        public bool PoisonedIsOn();
        public bool WeakIsOn();
        
        public void AddCondition(int condition);
        public void DelCondition(int condition);
    }
}