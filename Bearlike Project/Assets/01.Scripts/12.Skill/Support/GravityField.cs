using System.Collections;
using System.Collections.Generic;
using Fusion;
using GamePlay;
using Monster;
using Photon;
using Status;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.VFX;

namespace Skill.Support
{
    [RequireComponent(typeof(StatusBase))]
    public class GravityField : NetworkBehaviourEx
    {
        [Header("Status")]
        public StatusBase status;
     
        public float gravityPower = 10f;
        public float rotateStrength = 1f;
        public float gravityFieldDuration = 10f; // 중력장 지속 시간 

        [Header("VFX")] 
        public VisualEffect blackHoleVFX;
        public VisualEffect explodeVFX;

        private bool _isUpdate;
        
        private List<Rigidbody> _targetRigidBodyList = new List<Rigidbody>();
        private List<MonsterBase> _monsterList = new List<MonsterBase>();
        private TickTimer _durationTimer; // 지속시간 타이머
        
        private void OnTriggerEnter(Collider other)
        {
            Rigidbody rb;
            if (other.TryGetComponent(out ColliderStatus cs))
                rb = cs.originalStatus.GetComponent<Rigidbody>();
            else
                rb = other.attachedRigidbody;
            
            if (rb)
            {
                rb.isKinematic = false;
                
                if(other.TryGetComponent(out MonsterBase monster)) _monsterList.Add(monster);
                // if(rb.TryGetComponent(out NavMeshAgent navMeshAgent)) _navMeshAgentList.Add(navMeshAgent);
                _targetRigidBodyList.Add(rb);
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
                StartCoroutine(ExplodeCoroutine());
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
                var rb = _targetRigidBodyList[i];
                if(!rb) _targetRigidBodyList.RemoveAt(i);

                Vector3 dir = transform.position - rb.transform.position;
                Vector3 force = gravityPower * dir.normalized;
                
                Vector3 torqueDirection = Vector3.Cross(dir, Vector3.up);
                Vector3 rotationalForce = torqueDirection * rotateStrength;
                
                rb.AddForce(force);
                rb.AddTorque(rotationalForce);
            }
        }

        // 지속시간이 끝나고 대미지를 입히는 로직
        private void ApplyExplodeDamage()
        {
            foreach (var rb in _targetRigidBodyList)
            {
                if (rb)
                    rb.isKinematic = true;
            }
                
            foreach (var monster in _monsterList)
            {
                if (!monster) continue;
                monster.status.ApplyDamageRPC(status.CalDamage(), Object.Id);
            }
        }

        private IEnumerator ExplodeCoroutine()
        {
            var explodeTime = explodeVFX.GetFloat("ExplodeTime");
            yield return new WaitForSeconds(explodeTime);
            
            ApplyExplodeDamage();
            Destroy(gameObject);
        }
        
        [Rpc(RpcSources.All,RpcTargets.All)]
        public void OnExplodeVFXRPC() 
        {
            blackHoleVFX.Stop();
            explodeVFX.gameObject.SetActive(true);
        }
    }
}

