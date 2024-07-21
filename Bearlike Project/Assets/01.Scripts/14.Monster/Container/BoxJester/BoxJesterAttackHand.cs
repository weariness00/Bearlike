using System.Collections;
using DG.Tweening;
using Fusion;
using Manager;
using Photon;
using Status;
using UI.Status;
using UnityEngine;

namespace Monster.Container
{
    public class BoxJesterAttackHand : NetworkBehaviourEx
    {
        [Networked] public NetworkId OwnerId { get; set; }
        
        [SerializeField] private StatusBase status;
        [SerializeField]private GameObject[] hands;

        public Vector3 targetPosition;
        public Vector3 fakeTargetPosition;
        public int handType;
        public bool isFake;

        // Test용으로 이렇게 만든거임
        // 실제는 여기서 시간 설정해야함
        public float time;
        
        private void Awake()
        {
            var root = transform.root.gameObject.GetComponent<NetworkObject>();
            OwnerId = root.Id;

            status.damage.Max = 10;
            status.damage.Current = 10;
        }

        public override void Spawned()
        {
            base.Spawned();
            Destroy(gameObject, time);
            
            if(isFake)
                FakePunchAttackRPC(handType, targetPosition, fakeTargetPosition);
            else
                PunchAttackRPC(handType, targetPosition);
        }
        
        private void OnTriggerEnter(Collider other)
        {
            StatusBase otherStatus = null;

            if (true == other.gameObject.CompareTag("Player"))
            {
                if (other.gameObject.TryGetComponent(out otherStatus) ||
                    other.transform.root.gameObject.TryGetComponent(out otherStatus))
                {
                    status.AddAdditionalStatus(otherStatus);
                    // otherStatus.ApplyDamageRPC(status.CalDamage(out bool isCritical),
                    //     isCritical ? DamageTextType.Critical : DamageTextType.Normal, OwnerId);
                    otherStatus.ApplyDamageRPC(10, DamageTextType.Normal, OwnerId);
                    status.RemoveAdditionalStatus(otherStatus);
                }
            }
        }
        
        #region Punch Attack

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void PunchAttackRPC(int type, Vector3 targetPosition)
        {
            // TODO : Coroutine으로 해야하나
            hands[type].transform.LookAt(targetPosition);
            hands[type].transform.Rotate(90.0f, 0, 0);
            
            hands[type].transform.DOMove(targetPosition, time / 2).SetEase(Ease.InCirc); // TODO : 공격 속도를 변수처리 해야함
            
            StartCoroutine(ComeBackPunchCoroutine(type));
        }
            
        [Rpc(RpcSources.All, RpcTargets.All)]
        private void FakePunchAttackRPC(int type, Vector3 targetPosition, Vector3 fakeTargetPosition)
        {
            hands[type].transform.LookAt(fakeTargetPosition);
            hands[type].transform.Rotate(90.0f, 0, 0);
            
            hands[type].transform.DOMove(fakeTargetPosition, time / 4).SetEase(Ease.OutCirc); // TODO : 공격 속도를 변수처리 해야함

            StartCoroutine(RealTartgetMoveCoroutine(type, targetPosition));
            StartCoroutine(ComeBackPunchCoroutine(type));
        }

        private IEnumerator RealTartgetMoveCoroutine(int type, Vector3 targetPosition)
        {
            yield return new WaitForSeconds(time / 4);
            RealPunchAttackRPC(type, targetPosition);
        }
        
        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RealPunchAttackRPC(int type, Vector3 targetPosition)
        {
            hands[type].transform.LookAt(targetPosition);
            hands[type].transform.Rotate(90.0f, 0, 0);
            
            hands[type].transform.DOMove(targetPosition, time / 4).SetEase(Ease.InCirc); // TODO : 공격 속도를 변수처리 해야함
        }

        private IEnumerator ComeBackPunchCoroutine(int type)
        {
            yield return new WaitForSeconds(time / 2);

            ComeBackPunchRPC(type);
            yield return new WaitForSeconds(1.0f);
        }
        
        [Rpc(RpcSources.All, RpcTargets.All)]
        private void ComeBackPunchRPC(int type)
        {
            float tmp = 5.5f;
            if (type == 0)
                tmp = -5.5f;
            
            hands[type].transform.DOLocalMove(new Vector3(tmp, -6.5f, 7.9f), time / 2).SetEase(Ease.InCirc); // TODO : 공격 속도를 변수처리 해야함
        }

        #endregion
    }
}