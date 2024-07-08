using Player;
using Status;
using UnityEngine;

namespace User.MagicCotton.Container
{
    public class PowerUp : MagicCottonBase
    {
        public override void Apply(GameObject applyObj)
        {
            if (applyObj.TryGetComponent(out StatusBase status))
            {
                status.damageMultiple += Level.Current * 0.1f;
            }
        }
    }
}

