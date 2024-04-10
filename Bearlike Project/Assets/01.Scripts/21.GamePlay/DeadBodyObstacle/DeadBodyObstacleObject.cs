using System.Collections;
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
        private NavMeshObstacle[] _navMeshObstacles;

        private bool _isOn; // DeadBody가 활성화 되었는지
        
        #region Unity Event Function

        private void Awake()
        {
            _networkAnimator = GetComponent<NetworkMecanimAnimator>();
            _statusBase = GetComponent<StatusBase>();
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
            _ragdollRigidBodies = GetComponentsInChildren<Rigidbody>();
            _ragdollColliders = GetComponentsInChildren<Collider>();
            _navMeshObstacles = GetComponentsInChildren<NavMeshObstacle>();

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
                    !(component is NavMeshObstacle) &&
                    !(component is StatusBase))
                {
                    Destroy(component);
                }
            }

            // 레그돌 활성화
            SetDeadBodyComponentActive(true);

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
            
            // Nav Obstacle 활성화
            foreach (var navMeshObstacle in _navMeshObstacles)
            {
                navMeshObstacle.enabled = value;
                navMeshObstacle.carving = value;
            }

            if(_networkAnimator != null) _networkAnimator.Animator.enabled = !value;
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