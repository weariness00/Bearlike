using System;
using Fusion;
using Skill.Support;
using Status;
using UnityEngine;
using Util;

namespace Skill.Container
{
    [RequireComponent(typeof(StatusBase))]
    public class Domino : SkillBase
    {
        [Header("Domino 변수")]
        [SerializeField] private NetworkPrefabRef dominoPrefab;
        [SerializeField] private float spawnProbability;
        
        public override void Earn(GameObject earnTargetObject)
        {
            base.Earn(earnTargetObject);

            var statusData = GetStatusData(id);
            spawnProbability = statusData.GetFloat("Spawn Probability");
            
            status.AddAdditionalStatus(ownerPlayer.status);
            ownerPlayer.MonsterKillAction += SpawnDomino;
        }

        public override void MainLoop(){}
        public override void Run(){}

        private void SpawnDomino(GameObject monsterObj)
        {
            if (spawnProbability.IsProbability(1f))
            {
                var hitOptions = HitOptions.IncludePhysX | HitOptions.IgnoreInputAuthority; // 이 옵션을 포함하지 않으면 일반적인 물리 객체와 ray충돌 체크를 안함
                var includeRayMask = 1 << LayerMask.NameToLayer("Default");

                var spawnPosition = monsterObj.transform.position;
                
                if (Runner.LagCompensation.Raycast(spawnPosition, Vector3.up, 10f, Runner.LocalPlayer, out var lagHit, includeRayMask, hitOptions))
                {
                    spawnPosition = lagHit.Point;
                }
                else
                {
                    spawnPosition += Vector3.up * 10f;
                }
                
                Runner.SpawnAsync(dominoPrefab, spawnPosition, null, null, (runner, o) =>
                {
                    var dominoObject = o.GetComponent<DominoObject>();
                    dominoObject.SkillId = Object.Id;
                });
            }
        }

        public override void ExplainUpdate()
        {
            base.ExplainUpdate();
            if (explain.Contains("(Damage)")) explain = explain.Replace("(Damage)", $"{status.AddAllDamage()}");
            if (explain.Contains("(Level)")) explain = explain.Replace("(Level)", $"{level.Current}");
            if (explain.Contains("(Spawn Probability)")) explain = explain.Replace("(Spawn Probability)", $"{level.Current}");

            explain = explain.CalculateNumber();
        }
    }
}