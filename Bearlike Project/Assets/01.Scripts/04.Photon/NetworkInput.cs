﻿using Fusion;
using UnityEngine;

namespace Photon
{
    public struct PlayerInputData : INetworkInput
    {
        public Vector2 MouseAxis;

        public NetworkBool Escape;
        
        public NetworkBool MoveLeft;
        public NetworkBool MoveRight;
        public NetworkBool MoveBack;
        public NetworkBool MoveFront;

        public NetworkBool Dash;
        
        public NetworkBool Jump;

        public NetworkBool Interact;

        public NetworkBool Attack;
        public NetworkBool ReLoad;

        public NetworkBool ChangeWeapon0;
        public NetworkBool ChangeWeapon1;
        public NetworkBool ChangeWeapon2;

        public NetworkBool FirstSkill;
        public NetworkBool SecondSkill;
        public NetworkBool Ultimate;
    }
}
