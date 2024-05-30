using Fusion;
using Photon;
using UnityEngine;

namespace Test
{
    public class TestInterestPlayer : NetworkBehaviourEx, IInterestEnter
    {
        public void InterestEnter(PlayerRef player)
        {
            Debug.Log($"{name} : {player} 진입");
        }
    }
}

