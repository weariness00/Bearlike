using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using GamePlay;
using Manager;
using Monster;
using Photon;
using Player;
using Status;
using UI.Status;
using Unity.VisualScripting;
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

        private bool _isExplode;
        
        private HashSet<RigidBodyOriginInfo> _targetRigidBodyList = new HashSet<RigidBodyOriginInfo>(new RigidBodyOriginInfoComparer());
        private HashSet<MonsterBase> _monsterList = new HashSet<MonsterBase>();
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
            
            Rigidbody rb = other.GetComponentsInParent<Rigidbody>().Last();
            
            if (rb && rb.mass < gravityPower)
            {
                RigidBodyOriginInfo rbInfo = new RigidBodyOriginInfo()
                {
                    rigidbody = rb,
                    useGravity = rb.useGravity,
                    isKinematic = rb.isKinematic
                };

                if (rb.TryGetComponent(out MonsterBase monster))
                {
                    monster.DisableNavMeshAgent(false, false);
                    _monsterList.Add(monster);
                }
                else
                {
                    rb.isKinematic = false;
                }
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

            var rbInfo = new RigidBodyOriginInfo() { rigidbody = rb };
            
            if (_targetRigidBodyList.TryGetValue(rbInfo, out var currentRBInfo))
            {
                if(rb.TryGetComponent(out MonsterBase monster)) _monsterList.Remove(monster);
                _targetRigidBodyList.Remove(currentRBInfo);
                
                currentRBInfo.rigidbody.useGravity = currentRBInfo.useGravity;
                currentRBInfo.rigidbody.isKinematic = currentRBInfo.isKinematic;
            }
        }

        public override void Spawned()
        {
            base.Spawned();
            _isExplode = false;
            _durationTimer = TickTimer.CreateFromSeconds(Runner, gravityFieldDuration);
        }

        public override void FixedUpdateNetwork()
        {
            if (!_isExplode && _durationTimer.Expired(Runner))
            {
                _isExplode = true;
                OnExplodeVFXRPC();
            }
            PullTarget();
        }

        // 중력장에 의해 끌려오는 로직
        private void PullTarget()
        {
            foreach (var rbInfo in _targetRigidBodyList)
            {
                var rb = rbInfo.rigidbody;
                if (!rb) continue;

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
                monster.EnableNavMeshAgent();
                monster.status.ApplyDamageRPC(status.CalDamage(out var isCritical), isCritical ? DamageTextType.Critical : DamageTextType.Normal, Object.Id);
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
        
        public struct RigidBodyOriginInfo
        {
            public Rigidbody rigidbody;
            public bool useGravity;
            public bool isKinematic;
        }
        public class RigidBodyOriginInfoComparer : IEqualityComparer<RigidBodyOriginInfo>
        {
            // Equals 메서드에서 원하는 비교 로직을 구현
            public bool Equals(RigidBodyOriginInfo x, RigidBodyOriginInfo y)
            {
                // Id와 Name 모두 동일해야 같은 것으로 간주
                return x.rigidbody == y.rigidbody;
            }

            // GetHashCode 메서드에서 고유한 해시 코드를 반환
            public int GetHashCode(RigidBodyOriginInfo obj)
            {
                // 간단한 해시 코드 계산 (다른 방법으로 해시 코드를 생성할 수도 있음)
                int hash = 17;
                hash = hash * 31 + obj.rigidbody.GetHashCode();
                return hash;
            }
        }
    }
}

