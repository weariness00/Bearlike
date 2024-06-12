using Fusion;
using Skill.Support;
using Status;
using UnityEngine;
using UnityEngine.Serialization;
using Util;

namespace Skill.Container
{
    /// <summary>
    /// - 총알을 일정확률로 폭탄으로 변경해 주변에 스플래쉬 데미지를 준다.
    /// - 폭탄으로 변경된 총알의 대미지는 N%만큼 하락한다.
    /// - 폭탄의 스플레쉬 대미지는 기존 총알의 대미지에서 N%만큼 하락한 대미지이다.
    /// </summary>
    public class BulletIsBomb : SkillBase
    {
        [SerializeField] private NetworkPrefabRef bombPrefab;
        [SerializeField][Range(0f,1f)]private float bombProbability; // 폭탄으로 변환할 확률

        private bool _isExplode;

        public override void Awake()
        {
            base.Awake();

            var statusData = GetStatusData(id);
            bombProbability = statusData.GetFloat("Bomb Probability");
        }

        #region Memeber Function

        public override void Earn(GameObject earnTargetObject)
        {
            base.Earn(earnTargetObject);
            if (ownerPlayer.weaponSystem.TryGetEquipGun(out var gun))
            {
                gun.BeforeHitAction += BulletExplode;
                gun.AfterHitAction += AfterExplode;
            }
        }

        public override void MainLoop(){}

        public override void Run(){}

        public override void ExplainUpdate()
        {
            base.ExplainUpdate();
            if (explain.Contains("(Bomb Probability)"))
                explain = explain.Replace("(Bomb Probability)", $"{bombProbability  * 100f *level}");
            if (explain.Contains("(Damage Multiple)"))
                explain = explain.Replace("(Damage Multiple)", $"{(1 - status.damageMultiple) * 100f}");
            
            explain = StringExtension.CalculateNumber(explain);
        }

        private void BulletExplode(GameObject bulletObject, GameObject hitObject)
        {
            if (bombProbability.IsProbability(1f))
            {
                _isExplode = true;
                
                var bulletStatus = bulletObject.GetComponent<StatusBase>();
                bulletStatus.AddAdditionalStatus(status);
            }
        }

        // 폭탄으로 변경된후 뒷 처리
        private void AfterExplode(GameObject bulletObject, GameObject hitObject)
        {
            if (_isExplode)
            {
                _isExplode = false;
                
                var bulletStatus = bulletObject.GetComponent<StatusBase>();
                bulletStatus.RemoveAdditionalStatus(status); // 폭탄으로 변형이 성공되면서 변경된 스테이터스 제거하기
                
                // Bomb Object에서 총알의 Status와 스킬의 Status를 동시에 추가해주고 있기에 총알에서 스킬의 Status가 제거되면 소환 
                Runner.SpawnAsync(bombPrefab, bulletObject.transform.position, bulletObject.transform.rotation, null, (runner, o) =>
                {
                    var bomb = o.GetComponent<BulletBombObject>();

                    bomb.BulletIsBombId = Object.Id;
                    bomb.BulletOwnerId = bulletObject.GetComponent<NetworkObject>().Id;
                });
            }
        }
        
        #endregion
    }
}