using System;
using Fusion;
using Skill.Support;
using Status;
using UnityEngine;
using Util;
using Random = UnityEngine.Random;

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
                var rotate = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                
                Runner.SpawnAsync(dominoPrefab, monsterObj.transform.position, rotate, null, (runner, o) =>
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