using Fusion;
using Photon;
using UnityEngine;

namespace Test
{
    public class TestI : NetworkBehaviourEx, IInterestEnter, IInterestExit
    {
        public float extents = 32f;
        
        public override void FixedUpdateNetwork()
        {
            var controller = Object.InputAuthority;
            // Set the controlling players area of interest region around this object
            if (!controller.IsNone)
            {
                Runner.AddPlayerAreaOfInterest(controller, transform.position, extents);
            }
        }

        public void InterestEnter(PlayerRef player)
        {
            Debug.Log($"{player}가 영역에 들어옴");
        }

        public void InterestExit(PlayerRef player)
        {
            Debug.Log($"{player}가 영역에 나감");
        }
    }
}
