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
        private float _time;
        
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
            Destroy(gameObject, 3.0f);
            
            _time = time - 0.5f;
            
            PunchStartRPC();
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
        private void PunchStartRPC()
        {
            hands[handType].transform.DOLocalMove(hands[handType].transform.localPosition - hands[handType].transform.up * 3, 0.5f).SetEase(Ease.Linear);
            
            StartCoroutine(PunchCoroutine());
        }

        IEnumerator PunchCoroutine()
        {
            yield return new WaitForSeconds(0.5f);
            
            if(isFake)
                FakePunchAttackRPC();
            else
                PunchAttackRPC();
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void PunchAttackRPC()
        {
            hands[handType].transform.LookAt(targetPosition);
            hands[handType].transform.Rotate(90.0f, 0, 0);

            hands[handType].transform.DOMove(targetPosition, _time / 2).SetEase(Ease.InCirc);
            
            StartCoroutine(ComeBackPunchCoroutine());
        }
            
        [Rpc(RpcSources.All, RpcTargets.All)]
        private void FakePunchAttackRPC()
        {
            hands[handType].transform.LookAt(fakeTargetPosition);
            hands[handType].transform.Rotate(90.0f, 0, 0);

            hands[handType].transform.DOMove(fakeTargetPosition, _time / 4).SetEase(Ease.OutCirc);

            StartCoroutine(RealTartgetMoveCoroutine());
            StartCoroutine(ComeBackPunchCoroutine());
        }

        private IEnumerator RealTartgetMoveCoroutine()
        {
            yield return new WaitForSeconds(_time / 4);
            RealPunchAttackRPC();
        }
        
        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RealPunchAttackRPC()
        {
            hands[handType].transform.LookAt(targetPosition);
            hands[handType].transform.Rotate(90.0f, 0, 0);
            
            hands[handType].transform.DOMove(targetPosition, _time / 4).SetEase(Ease.InCirc);
        }

        private IEnumerator ComeBackPunchCoroutine()
        {
            yield return new WaitForSeconds(_time / 2);

            ComeBackPunchRPC();
            yield return new WaitForSeconds(1.0f);
        }
        
        [Rpc(RpcSources.All, RpcTargets.All)]
        private void ComeBackPunchRPC()
        {
            float tmp = 5.5f;
            if (handType == 0)
                tmp = -5.5f;
            
            hands[handType].transform.DOLocalMove(new Vector3(tmp, 6.5f, -8.1f), _time / 2).SetEase(Ease.InCirc);
        }

        #endregion
    }
}