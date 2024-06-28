using Fusion;
using Status;

namespace Monster.Container
{
    public class BoxJesterHat : NetworkBehaviour
    {
        [Networked] public NetworkId OwnerId { get; set; }

        public StatusBase status;

        public int hatType; // 0 : 정속성, 1 : Reverse
        
        public override void Spawned()
        {
            Destroy(gameObject, 10f);

            var ownerObj = Runner.FindObject(OwnerId);
            
            // BoxJester의 status에 모자의 개수를 저장해서 패턴을 구현할까?
            // 아니면 여기서 hp가 0이 되면 이벤트를 발생할까
        }
    }
}