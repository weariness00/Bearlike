using Fusion;
using Script.Util;
using Scripts.State.GameStatus;

namespace Script.Monster
{
    public class Monster : NetworkBehaviour
    {
        public Status status;

        private void Awake()
        {
            status = ObjectUtil.GetORAddComponet<Status>(gameObject);
        }
    }
}

