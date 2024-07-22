using System;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Player
{
    public class FirstBearRigController : MonoBehaviour
    {
        [SerializeField] private PlayerController ownerPlayer;
        
        [Header("Target")]
        public Transform rigTarget;
        public Vector3 rigTargetOffset;
        
        [Header("Rig")]
        public RigBuilder rigBuilder;

        private void Update()
        {
            rigTarget.position = ownerPlayer.transform.forward + rigTargetOffset;
        }
    }
}

