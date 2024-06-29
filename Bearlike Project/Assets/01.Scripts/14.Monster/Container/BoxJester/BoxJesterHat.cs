using Fusion;
using Photon;
using Status;
using Unity.VisualScripting;

namespace Monster.Container
{
    public class BoxJesterHat : NetworkBehaviourEx
    {
        [Networked] public NetworkId OwnerId { get; set; }

        public MonsterStatus status;
        public int hatType; // 0 : 정속성, 1 : Reverse

        private void Awake()
        {
            status = gameObject.GetOrAddComponent<MonsterStatus>();
            status.hp.Max = 100;
            status.hp.Current = 100;
        }
        
        public override void Spawned()
        {
            if(hatType == 1)
                status.AddCondition(CrowdControl.DamageReflect);
            
            Destroy(gameObject, 10f);

            var sd = OwnerId;
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
        
        // [Rpc(RpcSources.All, RpcTargets.All)]
        // private void BrokenHatRPC()
        // {
        //     var ownerObj = Runner.FindObject(OwnerId);
        //     var boxJester = ownerObj.gameObject.GetComponent<BoxJester>();
        //
        //     boxJester.hatCount--;
        // }
    }
}