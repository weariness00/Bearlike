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
        private NetworkMecanimAnimator _networkAnimator;
        private StatusBase _statusBase;
        private Rigidbody _rigidbody;
        private Collider _collider;
        private Rigidbody[] _ragdollRigidBodies;
        private Collider[] _ragdollColliders;

        private bool _isOn; // DeadBody가 활성화 되었는지
        
        #region Unity Event Function

        private void Awake()
        {
            _networkAnimator = GetComponent<NetworkMecanimAnimator>();
            _statusBase = GetComponent<StatusBase>();
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
            _ragdollRigidBodies = GetComponentsInChildren<Rigidbody>().Where(c => c.gameObject != gameObject).ToArray();
            _ragdollColliders = GetComponentsInChildren<Collider>().Where(c => c.gameObject != gameObject).ToArray();

            SetDeadBodyComponentActive(false);
        }

        public override void FixedUpdateNetwork()
        {
            if (_isOn && _statusBase.IsDie)
            {
                Destroy(gameObject);
            }
        }

        #endregion
        
        #region Member Function

        public void OnDeadBody(int hp = 1000)
        {
            name += "Dead Body";
            tag = "Default";
            gameObject.layer = 0;

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
                    !(component is StatusBase))
                {
                    Destroy(component);
                }
            }

            // 레그돌 활성화
            SetDeadBodyComponentActive(true);
            
            // Nav Mesh Obstacle 생성
            MakeNavMeshObstacle();

            // 시체의 체력 설정
            _statusBase.SetHpRPC(StatusValueType.CurrentAndMax, hp);
            StartCoroutine(CheckHpCoroutine()); // 시체에 정상적으로 hp가 부여되면 Update가 되도록 하는 코루틴
        }

        /// <summary>
        /// 레그돌 관련 컴포넌트 활성화/비활성화
        /// </summary>
        /// <param name="value"></param>
        private void SetDeadBodyComponentActive(bool value)
        {
            foreach (var rb in _ragdollRigidBodies)
            {
                rb.isKinematic = !value;
            }
            if (_rigidbody) _rigidbody.isKinematic = false;

            foreach (var col in _ragdollColliders)
            {
                col.enabled = value;
            }
            if (_collider) _collider.enabled = true;
            

            if(_networkAnimator != null) _networkAnimator.Animator.enabled = !value;
        }

        /// <summary>
        /// DeadBody(Rag Doll)의 Collider를 기반으로 Nav Mesh Obstacle을 생성해준다.
        /// Box, Capsule에 대해서만 생성해준다.
        /// </summary>
        private void MakeNavMeshObstacle()
        {
            List<NavMeshObstacle> navMeshObstacles = new List<NavMeshObstacle>();
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
            }
        }

        private IEnumerator CheckHpCoroutine()
        {
            while (true)
            {
                yield return null;
                if (_statusBase.hp.isMin == false)
                {
                    _isOn = true;
                    break;
                }
            }
        }

        #endregion

        #region RPC Function

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void OnDeadBodyRPC(int hp = 1000) => OnDeadBody(hp);

        #endregion
    }
}