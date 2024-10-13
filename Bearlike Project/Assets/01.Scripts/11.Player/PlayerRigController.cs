using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Fusion;
using GamePlay.Sync;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Player
{
    public class PlayerRigController : MonoBehaviour
    {
        [SerializeField] private PlayerController ownerPlayer;
        
        [Header("Aim")]
        public Transform aimParent;
        public GameObject targetAim;
        
        [Header("Rig")]
        public RigBuilder rigBuilder;
        [SerializeField] private Rig rig;

        [Header("Two Bone IK")] 
        [SerializeField] private TwoBoneIKConstraint leftArm;
        [SerializeField] private TwoBoneIKConstraint rightArm;
        
        [Header("Skinned Mesh")]
        [SerializeField] private GameObject armMehs;
        [SerializeField] private List<GameObject> otherMeshes;

        #region Parameter Getter & Setter

        public float RigWeight
        {
            get => rig.weight;
            set => rig.weight = value;
        }

        public float LeftArmWeight
        {
            get => leftArm.weight;
            set => leftArm.weight = value;
        }
        
        public float RightArmWeight
        {
            get => rightArm.weight;
            set => rightArm.weight = value;
        }
        
        #endregion

        private void Awake()
        {
            MakeAimObject();
        }

        private void Update()
        {
            AimPositionUpdate();
        }

        private void MakeAimObject()
        {
            targetAim ??= new GameObject
            {
                name = "Ray Aim",
                transform = { parent = aimParent}
            };
        }

        private void AimPositionUpdate()
        {
            if (Physics.Raycast(aimParent.position, aimParent.forward, out var hit))
            {
                targetAim.transform.position = hit.point;
            }
            else
            {
                targetAim.transform.position = aimParent.position + aimParent.forward * 2f;
            }
        }

        public void SetLeftArmIK(GameObject leftIK)
        {
            var sync = leftArm.data.target.GetComponent<TransformSync>();
            sync.targetTransform = leftIK.transform;
        }
        
        public void SetRightArmIK(GameObject rightIK)
        {
            var sync = rightArm.data.target.GetComponent<TransformSync>();
            sync.targetTransform = rightIK.transform;
        }

        public void EnableArmMesh()
        {
            armMehs.layer = LayerMask.NameToLayer("Weapon");
            foreach (var otherMesh in otherMeshes)
                otherMesh.layer = LayerMask.NameToLayer("Volum");
        }

        public void DisableArmMesh()
        {
            armMehs.layer = LayerMask.NameToLayer("Default");
            foreach (var otherMesh in otherMeshes)
                otherMesh.layer = LayerMask.NameToLayer("Default");
        }
    }
}

