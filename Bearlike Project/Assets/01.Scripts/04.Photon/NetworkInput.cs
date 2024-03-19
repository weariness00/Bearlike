﻿using Fusion;
using UnityEngine;

namespace Photon
{
    public struct PlayerInputData : INetworkInput
    {
        public Vector2 MouseAxis;
        
        public NetworkBool MoveLeft;
        public NetworkBool MoveRight;
        public NetworkBool MoveBack;
        public NetworkBool MoveFront;

        public NetworkBool Jump;

        public NetworkBool Interact;

        public NetworkBool Attack;
        public NetworkBool ReLoad;

        public NetworkBool ChangeWeapon0;

        public NetworkBool Cursor;

        public NetworkBool FirstSkill;
        public NetworkBool Ultimate;
    }
}