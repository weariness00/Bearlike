using System.Numerics;
using BehaviorTree.Base;
using Fusion;
using Photon;
using Status;
using Unity.VisualScripting;

namespace Monster.Container
{
    public class BoxJesterHat : MonsterBase
    {
        [Networked] public NetworkId OwnerId { get; set; }

        private BoxJester boxJester;
        
        public override void Awake()
        {
            base.Awake();
            status = gameObject.GetComponent<MonsterStatus>();
        }
        
        public override void Spawned()
        {
            base.Spawned();
            Destroy(gameObject, 10f);
            
            var ownerObj = Runner.FindObject(OwnerId);
            boxJester = ownerObj.gameObject.GetComponent<BoxJester>();

            DieAction += () =>
            {
                --boxJester.hatCount;
                Destroy(gameObject, 0f);
            };
        }
        
        public override INode InitBT()
        {
            throw new System.NotImplementedException();
        }
    }
}