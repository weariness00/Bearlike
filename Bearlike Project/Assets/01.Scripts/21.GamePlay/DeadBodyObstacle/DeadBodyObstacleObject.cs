using System;
using System.Collections;
using Fusion;
using Photon;
using State.StateClass.Base;
using Status;
using UnityEngine;

namespace GamePlay.DeadBodyObstacle
{
    public class DeadBodyObstacleObject : NetworkBehaviourEx
    {
        private NetworkMecanimAnimator _networkAnimator;
        private StatusBase _statusBase;
        private Rigidbody[] _ragdollRigidBodies;
        private Collider[] _ragdollColliders;

        private bool _isOn; // DeadBody가 활성화 되었는지
        
        #region Unity Event Function

        private void Awake()
        {
            _networkAnimator = GetComponent<NetworkMecanimAnimator>();
            _statusBase = GetComponent<StatusBase>();
            _ragdollRigidBodies = GetComponentsInChildren<Rigidbody>();
            _ragdollColliders = GetComponentsInChildren<Collider>();

            SetLagDoll(false);
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
            // 애니메이션 동작을 멈추기 위해 먼저 애니메이션 삭제
            Destroy(_networkAnimator.Animator);
            Destroy(_networkAnimator);
            
            // 특정 Componenet를 제외한 모든 Componenet 삭제
            Component[] components = GetComponents<Component>();
            foreach (var component in components)
            {
                if (!(component is MeshRenderer) &&
                    !(component is MeshFilter) &&
                    !(component is StatusBase))
                {
                    Destroy(component);
                }
            }

            // 레그돌 활성화
            SetLagDoll(true);

            _statusBase.SetHpRPC(StatusValueType.CurrentAndMax, hp);
            StartCoroutine(CheckHpCoroutine()); // 시체에 정상적으로 hp가 부여되면 Update가 되도록 하는 코루틴
        }

        /// <summary>
        /// 레그돌 관련 컴포넌트 활성화/비활성화
        /// </summary>
        /// <param name="value"></param>
        private void SetLagDoll(bool value)
        {
            foreach (var rb in _ragdollRigidBodies)
            {
                rb.isKinematic = !value;
            }

            foreach (var col in _ragdollColliders)
            {
                col.enabled = value;
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