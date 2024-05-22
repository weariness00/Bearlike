using System.Collections;
using System.Collections.Generic;
using Fusion;
using GamePlay;
using Monster;
using Photon;
using Player;
using Status;
using UnityEngine;
using UnityEngine.VFX;
using Random = UnityEngine.Random;

namespace Skill.Support
{
    [RequireComponent(typeof(StatusBase))]
    public class GravityField : NetworkBehaviourEx
    {
        [Header("Status")]
        public StatusBase status;
     
        public float gravityPower = 10f; // 중력장에 끓어올 수 있는 질량 최대치
        public float positionStrength = 1f; // 중력장에 끌려가는 힘
        public float rotateStrength = 1f; // 중력장에 끌려가면서 회전되는 힘
        public float explodeStrength = 10f; // 중력장이 터질때 내는 힘

        public float gravityFieldDuration = 10f; // 중력장 지속 시간 
        
        [Header("VFX")] 
        public VisualEffect blackHoleVFX;
        public GameObject explodeVFXObject;

        private bool _isUpdate;
        
        private List<RigidBodyOriginInfo> _targetRigidBodyList = new List<RigidBodyOriginInfo>();
        private List<MonsterBase> _monsterList = new List<MonsterBase>();
        private TickTimer _durationTimer; // 지속시간 타이머

        private PlayerCameraController _playerCameraController; // 플레이어가 진입시 카메라 떨림 효과 넣어주기 위해

        private void Start()
        {
            blackHoleVFX.SetFloat("Duration", gravityFieldDuration);
            blackHoleVFX.gameObject.SetActive(true);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.transform.root.name == "Local Player")
            {
                _playerCameraController = other.transform.root.GetComponent<PlayerCameraController>();
                _playerCameraController.ShakeCamera(gravityFieldDuration, Random.onUnitSphere, 2, 1f);
                return;
            }
            
            Rigidbody rb;
            if (other.TryGetComponent(out ColliderStatus cs))
                rb = cs.originalStatus.GetComponent<Rigidbody>();
            else
                rb = other.attachedRigidbody;
            
            if (rb && rb.mass < gravityPower)
            {
                RigidBodyOriginInfo rbInfo = new RigidBodyOriginInfo()
                {
                    rigidbody = rb,
                    useGravity = rb.useGravity,
                    isKinematic = rb.isKinematic
                };
                
                rb.isKinematic = false;
                
                if(rb.TryGetComponent(out MonsterBase monster)) _monsterList.Add(monster);
                // if(rb.TryGetComponent(out NavMeshAgent navMeshAgent)) _navMeshAgentList.Add(navMeshAgent);
                _targetRigidBodyList.Add(rbInfo);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.transform.root.name == "Local Player")
            {
                _playerCameraController.StopShake();
                _playerCameraController = null;
                return;
            }
            
            Rigidbody rb;
            if (other.TryGetComponent(out ColliderStatus cs))
                rb = cs.originalStatus.GetComponent<Rigidbody>();
            else
                rb = other.attachedRigidbody;
            
            if (rb && rb.mass < gravityPower)
            {
                if(rb.TryGetComponent(out MonsterBase monster)) _monsterList.Remove(monster);
                // if(rb.TryGetComponent(out NavMeshAgent navMeshAgent)) _navMeshAgentList.Add(navMeshAgent);
                for (var i = 0; i < _targetRigidBodyList.Count; i++)
                {
                    var rbInfo = _targetRigidBodyList[i];
                    if (rbInfo.rigidbody == rb)
                    {
                        _targetRigidBodyList.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        public override void Spawned()
        {
            base.Spawned();
            _isUpdate = true;
            _durationTimer = TickTimer.CreateFromSeconds(Runner, gravityFieldDuration);
        }

        public override void FixedUpdateNetwork()
        {
            if(_isUpdate == false) return;
            
            if (_durationTimer.Expired(Runner))
            {
                _isUpdate = false;
                OnExplodeVFXRPC();
                
            }
            else
            {
                PullTarget();
            }
        }

        // 중력장에 의해 끌려오는 로직
        private void PullTarget()
        {
            for (var i = 0; i < _targetRigidBodyList.Count; i++)
            {
                var rbInfo = _targetRigidBodyList[i];
                var rb = rbInfo.rigidbody;
                if (!rb)
                {
                    _targetRigidBodyList.RemoveAt(i);
                    continue;
                }

                Vector3 dir = transform.position - rb.transform.position;
                Vector3 force = positionStrength * dir.normalized;
                
                Vector3 torqueDirection = Vector3.Cross(dir, Vector3.up);
                Vector3 rotationalForce = torqueDirection * rotateStrength;
                
                rb.AddForce(force);
                rb.AddTorque(rotationalForce);
            }
        }

        // 지속시간이 끝나고 대미지를 입히는 로직
        private void ApplyExplodeDamage()
        {
            if (!HasStateAuthority) return;
            
            foreach (var rbInfo in _targetRigidBodyList)
            {
                if (rbInfo.rigidbody)
                {
                    var explodeDir = (rbInfo.rigidbody.transform.position - transform.position).normalized;
                    
                    rbInfo.rigidbody.AddForce(explodeStrength * explodeDir);
                    
                    rbInfo.rigidbody.useGravity = rbInfo.useGravity;
                    rbInfo.rigidbody.isKinematic = rbInfo.isKinematic;
                }
            }
                
            foreach (var monster in _monsterList)
            {
                if (!monster) continue;
                monster.UpdateNavMeshAgent();
                monster.status.ApplyDamageRPC(status.CalDamage(), Object.Id);
            }
        }

        private IEnumerator ExplodeCoroutine()
        {
            var oneSecondWait = new WaitForSeconds(1f);
            blackHoleVFX.Stop();
            yield return oneSecondWait;
            explodeVFXObject.SetActive(true);
            ApplyExplodeDamage();
            yield return oneSecondWait;
            Destroy(gameObject);
        }
        
        [Rpc(RpcSources.All,RpcTargets.All)]
        public void OnExplodeVFXRPC() 
        {
            StartCoroutine(ExplodeCoroutine());
        }
        
        private struct RigidBodyOriginInfo
        {
            public Rigidbody rigidbody;
            public bool useGravity;
            public bool isKinematic;
        }
    }
}

