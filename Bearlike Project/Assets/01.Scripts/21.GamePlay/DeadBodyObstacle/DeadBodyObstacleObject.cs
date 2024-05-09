using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Status;
using Fusion;
using Manager;
using Photon;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

namespace GamePlay.DeadBodyObstacle
{
    public class DeadBodyObstacleObject : NetworkBehaviourEx
    {
        public static NavMeshSurface stageSurface;

        public bool isOnStart = true;
        
        private NetworkMecanimAnimator _networkAnimator;
        private StatusBase _status;
        private Rigidbody _rigidbody;
        private Rigidbody[] _ragdollRigidBodies;
        private Collider[] _ragdollColliders;
        private List<NavMeshObstacle> _navMeshObstacleList;

        #region Unity Event Function

        private void Awake()
        {
            _networkAnimator = GetComponent<NetworkMecanimAnimator>();
            if(gameObject.TryGetComponent(out _status) == false) _status = GetComponent<StatusBase>();
            _rigidbody = GetComponent<Rigidbody>();
            _ragdollRigidBodies = GetComponentsInChildren<Rigidbody>().Where(c => c.gameObject != gameObject).ToArray();
            _ragdollColliders = GetComponentsInChildren<Collider>();

            SetDeadBodyComponentActive(false);
        }

        public override void Spawned()
        {
            base.Spawned();
            if(isOnStart) OnDeadBodyRPC();
        }

        public override void FixedUpdateNetwork()
        {

        }

        #endregion
        
        #region Member Function
        
        public void OnDeadBody(int hp = 1000)
        {
            name += "Dead Body";
            tag = "Untagged";
            gameObject.layer = LayerMask.NameToLayer("DeadBody");

            // 애니메이션 동작을 멈추기 위해 먼저 애니메이션 삭제
            if (_networkAnimator)
            {
                Destroy(_networkAnimator.Animator);
                Destroy(_networkAnimator);
            }

            // 특정 Componenet를 제외한 모든 Componenet 삭제
            Component[] components = GetComponents<Component>();
            foreach (var component in components)
            {
                if(component == null) continue;
                
                if (!(component is Transform) &&
                    !(component is MeshRenderer) &&
                    !(component is MeshFilter) &&
                    !(component is NetworkObject) &&
                    !(component is NetworkTransform) &&
                    !(component is StatusBase) && 
                    !(component is DeadBodyObstacleObject))
                {
                    Destroy(component);
                }
            }

            var colliderStatuses = GetComponentsInChildren<ColliderStatus>();
            foreach (var colliderStatus in colliderStatuses)
            {
                Destroy(colliderStatus);
            }

            // 레그돌 활성화
            SetDeadBodyComponentActive(true);
            
            _status.hp.Current = hp;

            if (HasStateAuthority)
            {
                // Nav Mesh Obstacle 생성
                MakeNavMeshObstacle();
                StartCoroutine(CheckHP());
                StartCoroutine(BakeNavMesh());
            }
        }

        /// <summary>
        /// 레그돌 관련 컴포넌트 활성화/비활성화
        /// </summary>
        /// <param name="value"></param>
        private void SetDeadBodyComponentActive(bool value)
        {
            foreach (var rb in _ragdollRigidBodies)
            {
                rb.useGravity = value;
                rb.constraints = value ? RigidbodyConstraints.None : RigidbodyConstraints.FreezeAll;
                if (value)
                    rb.constraints = 0;
            }

            foreach (var col in _ragdollColliders)
            {
                if(value)
                    col.gameObject.layer = LayerMask.NameToLayer("Ignore Nav Mesh");
            }

            if(_networkAnimator) _networkAnimator.Animator.enabled = !value;
        }

        /// <summary>
        /// DeadBody(Rag Doll)의 Collider를 기반으로 Nav Mesh Obstacle을 생성해준다.
        /// Box, Capsule에 대해서만 생성해준다.
        /// </summary>
        private void MakeNavMeshObstacle()
        {
            _navMeshObstacleList = new List<NavMeshObstacle>();
            foreach (var ragdollCollider in _ragdollColliders)
            {
                var obstacle = ragdollCollider.gameObject.AddComponent<NavMeshObstacle>();
                if (ragdollCollider is BoxCollider boxCollider)
                {
                    obstacle.shape = NavMeshObstacleShape.Box;
                    obstacle.center = boxCollider.center;
                    obstacle.size = boxCollider.size;
                }
                else if (ragdollCollider is CapsuleCollider capsuleCollider)
                {
                    obstacle.shape = NavMeshObstacleShape.Capsule;
                    obstacle.center = capsuleCollider.center;
                    obstacle.radius = capsuleCollider.radius;
                    obstacle.height = capsuleCollider.height;
                }

                obstacle.carving = true;
                obstacle.carveOnlyStationary = true;
                
                _navMeshObstacleList.Add(obstacle);
            }
        }

        private IEnumerator CheckHP()
        {
            while (true)
            {
                if (_status.IsDie)
                {
                    Destroy(gameObject);
                    break;
                }

                yield return null;
            }
        }

        private IEnumerator BakeNavMesh()
        {
            bool isAllActive = false;
            while (true)
            {
                yield return null;
                isAllActive = true;
                foreach (var obstacle in _navMeshObstacleList)
                {
                    if (obstacle.isActiveAndEnabled == false)
                    {
                        isAllActive = false;
                        break;
                    }
                }

                if (isAllActive)
                    break;
            }
            
            DebugManager.ToDo("Nav Mesh 리빌딩 하기");
            // if(stageSurface)
            //     stageSurface.BuildNavMesh();
        }

        #endregion

        #region RPC Function

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void OnDeadBodyRPC(int hp = 1000) => OnDeadBody(hp);

        #endregion
    }
}