using Fusion;
using Monster;
using Player;
using UI.Status;
using UnityEngine;
using UnityEngine.Serialization;

namespace Status
{
    /// <summary>
    /// Monster의 State을 나타내는 Class
    /// </summary>
    public class MonsterStatus : StatusBase
    {
        [HideInInspector] public MonsterBase monsterBase;
        
        private void Start()
        {
            InvokeRepeating(nameof(MainLoop), 0.0f, 1.0f);

            monsterBase = GetComponent<MonsterBase>();
        }

        #region Member Function
        
        public override void MainLoop()
        {
            if (ConditionPoisonedIsOn())
            {
                BePoisoned(Define.PoisonDamage);
                ShowInfo();
            }
        }
        
        public void BePoisoned(int value)
        {
            hp.Current -= value;
        }

        public override void ApplyDamage(int applyDamage, DamageTextType damageType, NetworkId ownerId, CrowdControl cc)
        {
            base.ApplyDamage(applyDamage, damageType, ownerId, cc);
            if (IsDie)
            {
                var obj = Runner.FindObject(ownerId);
                if(obj == null) return;
                if (obj.TryGetComponent(out PlayerController pc))
                {
                    pc.MonsterKillAction?.Invoke(gameObject);
                }
            }
        }

        public override void DamageText(int realDamage, DamageTextType type)
        {
            var randomDir = Random.insideUnitSphere;
            randomDir.y = Mathf.Abs(randomDir.y);
            DamageTextCanvas.SpawnDamageText(monsterBase.pivot.position + randomDir, realDamage, type);
        }

        #endregion

        #region Json Data Interfacec

        public override void SetJsonData(StatusJsonData json)
        {
            base.SetJsonData(json);
        }

        #endregion
    }
}