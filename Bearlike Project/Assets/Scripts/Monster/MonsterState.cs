using Fusion;
using State.StateClass.Base;
using Unity.Mathematics;
using Random = UnityEngine.Random;

namespace State.StateClass
{
    /// <summary>
    /// Monster의 State을 나타내는 Class
    /// </summary>
    public class MonsterState : Base.StateBase
    {
        public override void Initialization()
        {
            throw new System.NotImplementedException();
        }

        public override void MainLoop()
        {
            throw new System.NotImplementedException();
        }

        public override bool ApplyDamage(float damage, ObjectProperty property)
        {
            throw new System.NotImplementedException();
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