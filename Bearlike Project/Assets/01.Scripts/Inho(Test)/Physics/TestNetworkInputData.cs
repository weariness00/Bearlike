using Fusion;
using UnityEngine;

namespace Inho_Test_.Physics
{
    public struct TestNetworkInputData
    {
        public const byte MOUSEBUTTON0 = 1;
        public const byte MOUSEBUTTON1 = 2;

        public NetworkButtons buttons;
        public Vector3 direction;
    }
}