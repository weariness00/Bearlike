using System.Collections;
using Fusion;
using Photon;
using Status;
using UnityEngine;
using UnityEngine.VFX;

namespace Monster.Container
{
    public class ToySoldierGunAnimator : NetworkBehaviourEx
    {
        // 애니메이터 프로퍼티
        private static readonly int AniMoveSpeed = Animator.StringToHash("f Move Speed");
        private static readonly int AniAttack = Animator.StringToHash("tAttack");

        private ToySoldierGun toySoldierGun;
        
        [Header("Animator")]
        [SerializeField] private NetworkMecanimAnimator networkAnimator;
        
        [Header("Animation Clip")] 
        [SerializeField] private AnimationClip idleClip;
        [SerializeField] private AnimationClip moveClip;
        [SerializeField] private AnimationClip longAttackClip;

        [Header("ETC Component")]
        [SerializeField] private Transform gatherEnergyTransform;
        [SerializeField] private VisualEffect gatherEnergyVFX;
        
        private TickTimer AniIdleTimer { get; set; }
        private TickTimer AniMoveTimer { get; set; }
        private TickTimer AniLongAttackTimer { get; set; }
        
        public bool IdleTimerExpired => AniIdleTimer.Expired(Runner);
        public bool MoveTimerExpired => AniMoveTimer.Expired(Runner);
        public bool LongAttackTimerExpired => AniLongAttackTimer.Expired(Runner);

        public float MoveSpeed
        {
            get => networkAnimator.Animator.GetFloat(AniMoveSpeed);
            set => networkAnimator.Animator.SetFloat(AniMoveSpeed, value);
        }

        private void Awake()
        {
            toySoldierGun = GetComponentInParent<ToySoldierGun>();
            networkAnimator = GetComponent<NetworkMecanimAnimator>();
        }

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

        public void PlayLongAttack()
        {
            var lateTime = 0.5f;

            AniLongAttackTimer = TickTimer.CreateFromSeconds(Runner, longAttackClip.length + lateTime * 2);
            StartCoroutine(GunFireCoroutine(lateTime));
        }

        private IEnumerator GunFireCoroutine(float lateTime)
        {
            gatherEnergyVFX.transform.position = gatherEnergyTransform.position;
            gatherEnergyVFX.transform.rotation = gatherEnergyTransform.rotation;
            gatherEnergyVFX.SetFloat("Life Time", lateTime);
            gatherEnergyVFX.SetFloat("Circle Spawn Interval", lateTime / 3);
            gatherEnergyVFX.Play();
            
            var waitTime = new WaitForSeconds(lateTime);
            yield return waitTime;
            gatherEnergyVFX.Stop();
            yield return waitTime;
            
            networkAnimator.SetTrigger(AniAttack);

            toySoldierGun.gun.FireBullet(false);
            toySoldierGun.gun.SetMagazineRPC(StatusValueType.Current, 10);
            toySoldierGun.gun.FireLateTimer = TickTimer.CreateFromSeconds(Runner, 0);
        }
    }
}