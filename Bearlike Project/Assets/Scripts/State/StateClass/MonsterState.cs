// using Fusion;
// using Unity.Mathematics;
// using Random = UnityEngine.Random;
//
// namespace State.StateClass
// {
//     /// <summary>
//     /// Monster의 State을 나타내는 Class
//     /// </summary>
//     public class MonsterState : NetworkBehaviour, Base.IState
//     {
//         
//         public override void Initialization()
//         {
//         }
//
//         public override void MainLoop()
//         {
//         }
//
//         public override void BeDamaged(float damage)
//         {
//             if ((Random.Range(0.0f, 99.9f) < _avoid.Current)) return;
//             
//             var damageRate = math.log10((damage / _defence.Current) * 10);
//
//             if (WeakIsOn()) damageRate *= 1.5f;
//
//             _hp.Current -= (int)(damageRate * damage);
//         }
//
//         public override void ShowInfo()
//         {
//             throw new System.NotImplementedException();
//         }
//
//         public override bool On(int condition)
//         {
//             throw new System.NotImplementedException();
//         }
//
//         public override bool NormalityIsOn()
//         {
//             throw new System.NotImplementedException();
//         }
//
//         public override bool PoisonedIsOn()
//         {
//             throw new System.NotImplementedException();
//         }
//
//         public override bool WeakIsOn()
//         {
//             throw new System.NotImplementedException();
//         }
//
//         public override void AddCondition(int condition)
//         {
//             throw new System.NotImplementedException();
//         }
//
//         public override void DelCondition(int condition)
//         {
//             throw new System.NotImplementedException();
//         }
//     }
// }