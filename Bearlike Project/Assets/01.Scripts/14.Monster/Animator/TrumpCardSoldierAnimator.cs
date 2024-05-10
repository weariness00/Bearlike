using Photon;
using UnityEngine;

namespace Monster.Container
{
    public class TrumpCardSoldierAnimator : NetworkBehaviourEx
    {
        public TrumpCardSoldier monster;
        
        public void AniAttackRayEvent()
        {
            monster.AniAttackRayEvent();
        }
    }
}