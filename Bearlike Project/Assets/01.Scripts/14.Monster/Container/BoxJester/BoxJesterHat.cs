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

        public override void Awake()
        {
            base.Awake();
            status = gameObject.GetComponent<MonsterStatus>();
        }
        
        public override void Spawned()
        {
            Destroy(gameObject, 10f);
        }
        
        public override void FixedUpdateNetwork()
        {
            if (status.IsDie)
            {            
                var ownerObj = Runner.FindObject(OwnerId);

                var boxJester = ownerObj.gameObject.GetComponent<BoxJester>();
                boxJester.DestroyHatRPC();
                
                Destroy(gameObject, 0f);
            }
        }

        public override INode InitBT()
        {
            throw new System.NotImplementedException();
        }
    }
}