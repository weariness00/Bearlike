using Fusion;
using UnityEngine;

namespace _00.Scenes.Test___Dong_Woo__.Issue_Test
{
    public class MoveTest : NetworkBehaviour
    {
        public override void FixedUpdateNetwork()
        {
            if (HasStateAuthority)
            {
                transform.position += Vector3.forward * Runner.DeltaTime;
            }
        }
    }
}
