namespace Inho.Scripts.Equipment
{
    // public enum eEquitType
    // {
    //     Magnum,     // 매그넘
    //     Sniper,     // 저격총
    //     Shotgun,    // 샷건
    //     etc,        // 추가 예정
    //     Count
    // }
    
    public abstract class Equitment
    {
        protected float mDamage;
        protected int mMaxAmmu;         // 최대 탄창
        protected int mCurAmmu;         // 현재 탄창

        protected float mReroadTime;    // 장전 속도
        protected float mFireRate;      // 연사 속도

        public abstract void Init();

        public float GetDamage() { return mDamage; }
    }

    public class Magnum : Equitment
    {
        public override void Init()
        {
            mDamage = 10;
            mMaxAmmu = 8;
            mCurAmmu = 8;

            mReroadTime = 3.0f;
            mFireRate = 0.5f;
        }
    }
}