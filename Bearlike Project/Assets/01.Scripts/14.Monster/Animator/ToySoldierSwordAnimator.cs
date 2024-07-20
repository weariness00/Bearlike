using System.Collections;
using Fusion;
using Player;
using Status;
using UI.Status;
using UnityEngine;
using UnityEngine.VFX;
using Util.UnityEventComponent;

namespace Monster.Container
{
    public class ToySoldierSwordAnimator : NetworkBehaviour
    {
        // 애니메이터 프로퍼티
        private static readonly int AniMoveSpeed = Animator.StringToHash("f Move Speed");
        private static readonly int AniDefaultAttack = Animator.StringToHash("tAttack");
        private static readonly int AniStabbingAttack = Animator.StringToHash("tAttack2");
        private static readonly int AniAttackSpeed = Animator.StringToHash("f Attack Speed");

        private ToySoldierSword toySoldierSword;
        private NetworkMecanimAnimator networkAnimator;
        
        [Header("Animation Clip")] 
        [SerializeField] private AnimationClip idleClip;
        [SerializeField] private AnimationClip moveClip;
        [SerializeField] private AnimationClip defaultAttackClip;
        [SerializeField] private AnimationClip stabbingAttackClip;

        [Header("VFX")]
        [SerializeField] private VisualEffect defaultAttackVFX;
        [SerializeField] private VisualEffect stabbingAttackVFX;
        [SerializeField] private VisualEffect gatherEnergyVFX;

        [Header("ETC Component")] 
        [SerializeField] private Transform stabbingVFXTransform; // 찌르는 VFX가 생성될 위치
        [SerializeField] private Collider defaultCollider;
        [SerializeField] private Collider stabbingCollider;
        
        private TickTimer AniIdleTimer { get; set; }
        private TickTimer AniMoveTimer { get; set; }
        private TickTimer AniDefaultAttackTimer { get; set; }
        private TickTimer AniStabbingAttackTimer { get; set; }
        
        public bool IdleTimerExpired => AniIdleTimer.Expired(Runner);
        public bool MoveTimerExpired => AniMoveTimer.Expired(Runner);
        public bool DefaultAttackTimerExpired => AniDefaultAttackTimer.Expired(Runner);
        public bool StabbingAttackTimerExpired => AniStabbingAttackTimer.Expired(Runner);

        public float MoveSpeed
        {
            get => networkAnimator.Animator.GetFloat(AniMoveSpeed);
            set => networkAnimator.Animator.SetFloat(AniMoveSpeed, value);
        }

        public float AttackSpeed
        {
            get => networkAnimator.Animator.GetFloat(AniAttackSpeed);
            set => networkAnimator.Animator.SetFloat(AniAttackSpeed, value);
        }

        #region Unity Evenet Function

        private void Awake()
        {
            toySoldierSword = GetComponentInParent<ToySoldierSword>();
            networkAnimator = GetComponent<NetworkMecanimAnimator>();
        }

        private void Start()
        {
            // Collider 초기화
            // Deadbody로 인해 무조건 Start에서 초기화 해야된다.
            defaultCollider.gameObject.layer = 0;
            defaultCollider.gameObject.tag = "Default";
            stabbingCollider.gameObject.layer = 0;
            stabbingCollider.gameObject.tag = "Default";
            
            defaultCollider.enabled = false;
            stabbingCollider.enabled = false;
        }

        public override void Spawned()
        {
            base.Spawned();
            if (HasStateAuthority)
            {
                {
                    var util = defaultCollider.gameObject.AddComponent<UnityEventUtil>();
                    util.AddOnTriggerEnter(DefaultAttackOnTriggerEnter);
                }
                {
                    var util = stabbingCollider.gameObject.AddComponent<UnityEventUtil>();
                    util.AddOnTriggerEnter(StabbingAttackOnTriggerEnter);
                }
            }
        }
        

        #endregion

        public void PlayIdle()
        {
            AniIdleTimer = TickTimer.CreateFromSeconds(Runner, idleClip.length);
            MoveSpeed = 0f;
        }

        public void PlayMove()
        {
            AniMoveTimer = TickTimer.CreateFromSeconds(Runner, moveClip.length);
            MoveSpeed = 1f;
        }

        public void PlayDefaultAttack()
        {
            AniDefaultAttackTimer = TickTimer.CreateFromSeconds(Runner, defaultAttackClip.length / AttackSpeed);
            networkAnimator.SetTrigger(AniDefaultAttack);
        }

        public void PlayStabbingAttack()
        {
            AniStabbingAttackTimer = TickTimer.CreateFromSeconds(Runner, stabbingAttackClip.length / AttackSpeed);
            networkAnimator.SetTrigger(AniStabbingAttack);
        }

        #region Animation Clip Event Function

        #region Default Attack

        private void DefaultAttackStartEvent()
        {
            gatherEnergyVFX.transform.position = toySoldierSword.weaponTransform.position;
            gatherEnergyVFX.transform.rotation = toySoldierSword.weaponTransform.rotation;

            gatherEnergyVFX.Play();

            defaultCollider.enabled = true;
        }
        
        private void DefaultAttackEndEvent()
        {
            defaultAttackVFX.transform.position = toySoldierSword.weaponTransform.position;
            defaultAttackVFX.transform.rotation = toySoldierSword.weaponTransform.rotation;
            
            defaultAttackVFX.Play();
            gatherEnergyVFX.Stop();
            
            defaultCollider.enabled = false;
        }

        private void DefaultAttackOnTriggerEnter(Collider other)
        {
            if (defaultCollider.enabled)
            {
                var otherStatus = other.GetComponentInParent<StatusBase>();
                if (otherStatus is PlayerStatus)
                {
                    otherStatus.ApplyDamageRPC(
                        toySoldierSword.status.CalDamage(out bool isCritical),
                        isCritical ? DamageTextType.Critical : DamageTextType.Normal,
                        Object.Id);
                }
            }
        }

        #endregion

        #region Stabbing Attack

        private void StabbingAttackStartEvent()
        {
            stabbingAttackVFX.transform.position = stabbingVFXTransform.position;
            stabbingAttackVFX.transform.rotation = stabbingVFXTransform.rotation;
            
            var frame = 0.1f / toySoldierSword.status.attackSpeed.Current;
            stabbingAttackVFX.SetFloat("Speed", frame);
            stabbingAttackVFX.SetVector3("Velocity", Vector3.zero);
            stabbingAttackVFX.Play();

            stabbingCollider.enabled = true;

            StartCoroutine(StabbingMove());
        }
        
        private void StabbingAttackEndEvent()
        {
            stabbingAttackVFX.Stop();
            
            StopCoroutine(StabbingMove());
            
            stabbingCollider.enabled = false;
            
            toySoldierSword.rigidbody.velocity = Vector3.zero;
            toySoldierSword.rigidbody.angularVelocity = Vector3.zero;
        }

        // 찌르기 할때 장난감 병정을 이동시키는 로직
        private IEnumerator StabbingMove()
        {
            var frame = 0.1f / toySoldierSword.status.attackSpeed.Current;
            var acc = toySoldierSword.stabbingDistance / frame;
            var force = toySoldierSword.rigidbody.mass * acc;
            while (frame > 0)
            {
                frame -= Time.deltaTime;
                
                toySoldierSword.rigidbody.AddForce(10f * force * toySoldierSword.pivot.forward);
                
                stabbingAttackVFX.transform.position = stabbingVFXTransform.position;
                stabbingAttackVFX.transform.rotation = stabbingVFXTransform.rotation;
                    
                yield return null;
            }
        }

        private void GatherEnergyStartEvent()
        {
            gatherEnergyVFX.transform.position = toySoldierSword.weaponTransform.position;
            gatherEnergyVFX.transform.rotation = Quaternion.identity;
            
            gatherEnergyVFX.Play();
        }
        
        private void GatherEnergyEndEvent()
        {
            gatherEnergyVFX.Stop();
        }

        private void StabbingAttackOnTriggerEnter(Collider other)
        {
            if (stabbingCollider.enabled)
            {
                var otherStatus = other.GetComponentInParent<StatusBase>();
                if (otherStatus is PlayerStatus)
                {
                    otherStatus.ApplyDamageRPC(
                        (int)(toySoldierSword.stabbingAttackDamageMultiple * toySoldierSword.status.CalDamage(out bool isCritical)),
                        isCritical ? DamageTextType.Critical : DamageTextType.Normal,
                        Object.Id);
                }
            }
        }
        
        #endregion

        #endregion
    }
}