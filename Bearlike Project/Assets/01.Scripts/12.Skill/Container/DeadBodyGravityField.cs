using Fusion;
using Manager;
using Monster;
using Player;
using Skill.Support;
using Unity.VisualScripting;
using UnityEngine;

namespace Skill.Container
{
    public class DeadBodyGravityField : SkillBase
    {
        public NetworkPrefabRef gravityFieldPrefab;
        public float gravityPower = 10f; // 중력장에 끓어올 수 있는 질량 최대치
        public float positionStrength = 1f; // 중력장에 끌려가는 힘
        public float rotateStrength = 1f; // 중력장에 끌려가면서 회전되는 힘
        public float gravityFieldDuration = 10f; // 중력장 지속 시간 

        #region Unity Envet Function

        public override void Awake()
        {
            base.Awake();
            var data = GetStatusData(id);
            gravityPower = data.GetFloat("Gravity Power");
            positionStrength = data.GetFloat("Gravity Position Power");
            rotateStrength = data.GetFloat("Gravity Rotate Power");
        }

        #endregion

        #region Skill Function

        public override void MainLoop()
        {
        }

        public override void Run()
        {
        }

        public override void Earn(GameObject earnTargetObject)
        {
            base.Earn(earnTargetObject);
            if (earnTargetObject.TryGetComponent(out PlayerController pc))
            {
                pc.MonsterKillAction -= SpawnGravityField;
                pc.MonsterKillAction += SpawnGravityField;
            }
        }
        
        #endregion

        #region Member Function

        private void SpawnGravityField(GameObject targetObject)
        {
            if (HasStateAuthority)
            {
                var monster = targetObject.GetComponent<MonsterBase>();
                Runner.SpawnAsync(gravityFieldPrefab, monster.pivot.position, null, null, (runner, o) =>
                {
                    var gf = o.GetComponent<GravityField>();
                    gf.gravityPower = gravityPower;
                    gf.rotateStrength = rotateStrength;
                    gf.gravityFieldDuration = gravityFieldDuration;

                    gf.status.damage.Max = status.damage.Max;
                    gf.status.damage.Current = status.CalDamage();
                });
                
                DebugManager.Log($"{targetObject.name}에 중력장 소환");
            }
        }

        #endregion
    }
}

