using Fusion;
using UnityEngine;

namespace GamePlay
{
    // A에서 B로 넘어갈 수 있는 포털
    public class Portal : MonoBehaviour
    {
        #region Networked Variable

        [Networked] public NetworkBool IsConnect { get; set; } // 포탈과 연결 되었는지

        #endregion
        
        public Portal otherPortal;
    }
}

