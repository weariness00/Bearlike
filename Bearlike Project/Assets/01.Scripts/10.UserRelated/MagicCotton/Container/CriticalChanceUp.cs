using Status;
using UnityEngine;
using User.MagicCotton;

namespace UserRelated.MagicCotton.Container
{
    public class CriticalChanceUp : MagicCottonBase
    {
        public override void Apply(GameObject applyObj)
        {
            if (applyObj.TryGetComponent(out StatusBase status))
            {
                status.criticalHitChance.Current += Level.Current * 0.1f;
            }
        }
    }
}

