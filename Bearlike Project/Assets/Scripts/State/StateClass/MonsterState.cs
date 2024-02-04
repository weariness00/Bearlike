using State.StateClass.Base;
using Unity.Mathematics;
using Random = UnityEngine.Random;

namespace State.StateClass
{
    /// <summary>
    /// Monster의 State을 나타내는 Class
    /// </summary>
    public class MonsterState : Base.State
    {
        // Member Variable
        #region Info Perperty
        
        #endregion
        
        public override void Initialization()
        {
            
        }

        public override void MainLoop()
        {
            
        }

        public override bool ApplyDamage(float damage, ObjectProperty property)
        {
            if (hp.Current < 0)
            {
                return false;
            }

            if ((Random.Range(0.0f, 99.9f) < avoid.Current))
            {
                return false;
            }
            
            AddCondition(property);         // Monster의 속성을 Player상태에 적용
            
            var damageRate = math.log10((damage / defence.Current) * 10);

            if (WeakIsOn())
            {
                damageRate *= 1.5f;
            }

            hp.Current -= (int)(damageRate * damage);
                
            return true;
        }

        public override void ShowInfo()
        {
            throw new System.NotImplementedException();
        }

        public override bool On(ObjectProperty condition)
        {
            throw new System.NotImplementedException();
        }

        public override bool NormalityIsOn()
        {
            throw new System.NotImplementedException();
        }

        public override bool PoisonedIsOn()
        {
            throw new System.NotImplementedException();
        }

        public override bool WeakIsOn()
        {
            throw new System.NotImplementedException();
        }

        public override void AddCondition(ObjectProperty condition)
        {
            throw new System.NotImplementedException();
        }

        public override void DelCondition(ObjectProperty condition)
        {
            throw new System.NotImplementedException();
        }
    }
}