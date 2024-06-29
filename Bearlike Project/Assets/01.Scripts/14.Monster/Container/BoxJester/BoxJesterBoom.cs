using Fusion;
using Photon;
using Status;

namespace Monster.Container
{
    public class BoxJesterBoom : NetworkBehaviourEx
    {
        [Networked] public NetworkId OwnerId { get; set; }

        public MonsterStatus status;
        
        
    }
}