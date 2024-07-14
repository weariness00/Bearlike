using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Status;
using Fusion;
using Photon;
using UnityEngine;
using UnityEngine.AI;

namespace GamePlay.DeadBodyObstacle
{
    public class DeadBodyObstacleObject : NetworkBehaviourEx
    {
        public bool isOnStart = true;
        
        private NetworkMecanimAnimator _networkAnimator;
        private StatusBase _status;
        private Rigidbody _rigidbody;
        private Rigidbody[] _ragdollRigidBodies;
        private Collider[] _ragdollColliders;
        private List<NavMeshObstacle> _navMeshObstacleList;

        private float movementThreshold = 0.01f;
        
        #region Unity Event Function

        private void Awake()
        {
            _networkAnimator = GetComponentInChildren<NetworkMecanimAnimator>();
            if(gameObject.TryGetComponent(out _status) == false) _status = GetComponent<StatusBase>();
            _rigidbody = GetComponent<Rigidbody>();
            _ragdollRigidBodies = GetComponentsInChildren<Rigidbody>().Where(c => c.gameObject != gameObject).ToArray();
            _ragdollColliders = GetComponentsInChildren<Collider>();

            SetDeadBodyComponentActive(false);
            SetLagDollLayer(LayerMask.NameToLayer("Ignore Nav Mesh"));
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
            SetLagDollLayer(LayerMask.NameToLayer("DeadBody"));
            
            _status.hp.Current = hp;

            MakeNetworkTransform();
            NavMeshRebuildSystem.ReBuild();
            if (HasStateAuthority)
            {
                InvokeRepeating(nameof(BakeNavMeshToCollider), 1,0.1f);
                StartCoroutine(CheckHP());
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
            }

            if(_networkAnimator) _networkAnimator.Animator.enabled = !value;
        }

        private void SetLagDollLayer(LayerMask mask)
        {
            foreach (var col in _ragdollColliders)
                col.gameObject.layer = mask;
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
        
        /// <summary>
        /// Rigid body를 기준으로 Network Transform 컴포넌트 부착
        /// </summary>
        private void MakeNetworkTransform()
        {
            foreach (var rb in _ragdollRigidBodies)
            {
                rb.gameObject.AddComponent<NetworkTransform>();
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
        
        /// <summary>
        /// RigidBody의 움직임이 멈추었는지 판단하고 NavMesh ReBuild
        /// </summary>
        /// <returns></returns>
        private IEnumerator BakeNavMeshToCollider()
        {
            var updateTime = new WaitForSeconds(0.1f);
            
            float stopThreshold = 0.1f; // 멈춤을 감지할 속도 임계값
            float stopDuration = 2.0f; // 멈춤을 판단하기 위한 지속 시간
            while (true)
            {
                yield return updateTime;
                
                float totalVelocity = 0.0f;

                // 모든 Rigidbody의 속도를 합산합니다.
                foreach (Rigidbody rb in _ragdollRigidBodies)
                {
                    totalVelocity += rb.velocity.magnitude;
                }

                // 평균 속도를 계산합니다.
                float averageVelocity = totalVelocity / _ragdollRigidBodies.Length;

                // 평균 속도가 임계값보다 낮으면 멈춘 것으로 간주합니다.
                if (averageVelocity < stopThreshold)
                    break;
            }

            SetDeadBodyComponentActive(false);
            NavMeshRebuildSystem.ReBuildRPC();
        }

        #endregion

        #region RPC Function

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void OnDeadBodyRPC(int hp = 1000) => OnDeadBody(hp);

        #endregion
    }
}