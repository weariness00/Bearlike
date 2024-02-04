using Fusion;
using Script.Util;
using Scripts.State.GameStatus;
using Unity.VisualScripting;

namespace Script.Monster
{
    public class Monster : NetworkBehaviour
    {
        public Status status;

        private void Awake()
        {
            status = gameObject.GetOrAddComponent<Status>();
        }
    }
}

