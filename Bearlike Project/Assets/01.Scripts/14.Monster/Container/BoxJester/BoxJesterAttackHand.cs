using System;
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
        
        [Networked] public Vector3 targetPosition { get; set; }
        [Networked] public Vector3 fakeTargetPosition { get; set; }
        [Networked] public int handType { get; set; }
        [Networked] public bool isFake { get; set; }
        
        // Test용으로 이렇게 만든거임
        // 실제는 여기서 시간 설정해야함
        public float time;
        private float _time;
        
        private void Awake()
        {
            status.damage.Max = 10;
            status.damage.Current = 10;
        }
        
        public override void Spawned()
        {
            base.Spawned();
            Destroy(gameObject, 3.0f);
            
            var root = transform.root.gameObject.GetComponent<NetworkObject>();
            OwnerId = root.Id;
            
            _time = time - 0.5f;

            StartCoroutine(StartPunchingCoroutine());
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

        private void Update()
        {
            DebugManager.Log($"posiiton : {hands[handType].transform.position}");
        }

        #region Punch Attack

        IEnumerator StartPunchingCoroutine()
        {
            float time = 0.0f;
            Quaternion targetRotation =
                Quaternion.LookRotation((targetPosition - hands[handType].transform.position).normalized);

            var hanTrans = hands[handType].transform;
            
            while (time < 1.0f)
            {
                hanTrans.position = Vector3.Slerp(hanTrans.position, hanTrans.position - hanTrans.up * 2, time);
                // hanTrans.rotation = Quaternion.Slerp(hanTrans.rotation, targetRotation, time);
                
                time += 2 * Time.deltaTime;
                yield return null;
            }
            
            if (isFake)
                StartCoroutine(FakePunchAttackCoroutine());
            else
                StartCoroutine(PunchAttackCoroutine());
        }

        IEnumerator PunchAttackCoroutine()
        {
            hands[handType].transform.LookAt(targetPosition);
            hands[handType].transform.Rotate(90.0f, 0, 0);

            float time = 0.0f;
            
            var hanTrans = hands[handType].transform;

            while (time < 1.0f)
            {
                hanTrans.position = Vector3.Slerp(hanTrans.position, targetPosition, time);

                time += Time.deltaTime / (_time / 2);
                yield return null;
            }
            
            StartCoroutine(ComeBackPunchCoroutine());
        }
        
        IEnumerator FakePunchAttackCoroutine()
        {
            hands[handType].transform.LookAt(fakeTargetPosition);
            hands[handType].transform.Rotate(90.0f, 0, 0);

            float time = 0.0f;
            
            var hanTrans = hands[handType].transform;

            while (time < 1.0f)
            {
                hanTrans.position = Vector3.Slerp(hanTrans.position, fakeTargetPosition, time);

                time += Time.deltaTime / (_time / 4);
                yield return null;
            }
            
            StartCoroutine(RealTartgetMoveCoroutine());
        }
        
        private IEnumerator RealTartgetMoveCoroutine()
        {
            hands[handType].transform.LookAt(targetPosition);
            hands[handType].transform.Rotate(90.0f, 0, 0);

            float time = 0.0f;
            
            var hanTrans = hands[handType].transform;

            while (time < 1.0f)
            {
                hanTrans.position = Vector3.Slerp(hanTrans.position, targetPosition, time);

                time += Time.deltaTime / (_time / 4);
                yield return null;
            }
            
            StartCoroutine(ComeBackPunchCoroutine());
        }
        
        
        private IEnumerator ComeBackPunchCoroutine()
        {
            float tmp = 5.5f;
            if (handType == 0)
                tmp = -5.5f;
            
            float time = 0.0f;
            
            var hanTrans = hands[handType].transform;

            while (time < 1.0f)
            {
                hanTrans.localPosition = Vector3.Slerp(hanTrans.localPosition, new Vector3(tmp, 6.5f, -8.1f), time);

                time += Time.deltaTime / (_time / 2);
                yield return null;
            }
        }
        
        #endregion
        
        // [Networked] public Vector3 position { get; set; }
        // [Networked] public Quaternion rotation { get; set; }
        //
        // [Networked] public Vector3 targetPosition { get; set; }
        // [Networked] public Vector3 fakeTargetPosition { get; set; }
        // [Networked] public int handType { get; set; }
        // [Networked] public bool isFake { get; set; }
        //
        // // Test용으로 이렇게 만든거임
        // // 실제는 여기서 시간 설정해야함
        // public float time;
        // private float _time;
        //
        // private void Awake()
        // {
        //     status.damage.Max = 10;
        //     status.damage.Current = 10;
        // }
        //
        // public override void Spawned()
        // {
        //     base.Spawned();
        //     Destroy(gameObject, 3.0f);
        //     
        //     var root = transform.root.gameObject.GetComponent<NetworkObject>();
        //     OwnerId = root.Id;
        //
        //     DebugManager.Log($"position : {position}, rotation : {rotation}");
        //     
        //     transform.position = position;
        //     transform.rotation = rotation;
        //     
        //     _time = time - 0.5f;
        //     
        //     PunchStart();
        // }
        //
        // private void Update()
        // {
        //     DebugManager.Log($"position : {hands[handType].transform.position}");
        // }
        //
        // private void OnTriggerEnter(Collider other)
        // {
        //     StatusBase otherStatus = null;
        //
        //     if (true == other.gameObject.CompareTag("Player"))
        //     {
        //         if (other.gameObject.TryGetComponent(out otherStatus) ||
        //             other.transform.root.gameObject.TryGetComponent(out otherStatus))
        //         {
        //             status.AddAdditionalStatus(otherStatus);
        //             // otherStatus.ApplyDamageRPC(status.CalDamage(out bool isCritical),
        //             //     isCritical ? DamageTextType.Critical : DamageTextType.Normal, OwnerId);
        //             otherStatus.ApplyDamageRPC(10, DamageTextType.Normal, OwnerId);
        //             status.RemoveAdditionalStatus(otherStatus);
        //         }
        //     }
        // }
        //
        // #region Punch Attack
        //
        // private void PunchStart()
        // {
        //     hands[handType].transform.DOMove(hands[handType].transform.position - hands[handType].transform.up * 3, 0.5f).SetEase(Ease.Linear);
        //     
        //     StartCoroutine(PunchCoroutine());
        // }
        //
        // IEnumerator PunchCoroutine()
        // {
        //     yield return new WaitForSeconds(0.5f);
        //     
        //     if(isFake)
        //         FakePunchAttack();
        //     else
        //         PunchAttack();
        // }
        //
        // private void PunchAttack()
        // {
        //     hands[handType].transform.LookAt(targetPosition);
        //     hands[handType].transform.Rotate(90.0f, 0, 0);
        //
        //     hands[handType].transform.DOMove(targetPosition, _time / 2).SetEase(Ease.InCirc);
        //     
        //     StartCoroutine(ComeBackPunchCoroutine());
        // }
        //     
        // private void FakePunchAttack()
        // {
        //     hands[handType].transform.LookAt(fakeTargetPosition);
        //     hands[handType].transform.Rotate(90.0f, 0, 0);
        //
        //     hands[handType].transform.DOMove(fakeTargetPosition, _time / 4).SetEase(Ease.OutCirc);
        //
        //     StartCoroutine(RealTartgetMoveCoroutine());
        //     StartCoroutine(ComeBackPunchCoroutine());
        // }
        //
        // private IEnumerator RealTartgetMoveCoroutine()
        // {
        //     yield return new WaitForSeconds(_time / 4);
        //     RealPunchAttack();
        // }
        //
        // private void RealPunchAttack()
        // {
        //     hands[handType].transform.LookAt(targetPosition);
        //     hands[handType].transform.Rotate(90.0f, 0, 0);
        //     
        //     hands[handType].transform.DOMove(targetPosition, _time / 4).SetEase(Ease.InCirc);
        // }
        //
        // private IEnumerator ComeBackPunchCoroutine()
        // {
        //     yield return new WaitForSeconds(_time / 2);
        //
        //     ComeBackPunch();
        // }
        //
        // private void ComeBackPunch()
        // {
        //     float tmp = 5.5f;
        //     if (handType == 0)
        //         tmp = -5.5f;
        //     
        //     hands[handType].transform.DOLocalMove(new Vector3(tmp, 6.5f, -8.1f), _time / 2).SetEase(Ease.InCirc);
        // }
        //
        // #endregion
    }
}