using Aggro;
using Fusion;
using Monster;
using Player;
using UI.Status;
using UnityEngine;

namespace Status
{
    /// <summary>
    /// Monster의 State을 나타내는 Class
    /// </summary>
    public class MonsterStatus : StatusBase
    {
        [HideInInspector] public MonsterBase monsterBase;

        private bool isInvokeKillAction = false;
        
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
                if(!obj.gameObject) return;
                
                if (!isInvokeKillAction && obj.TryGetComponent(out PlayerController pc))
                {
                    isInvokeKillAction = true;
                    pc.MonsterKillAction?.Invoke(gameObject);
                }
            }
            else
            {
                // 어그로 대상이 없는 상태에서 공격을 받으면 해당 대상이 어그로로 잡힘
                if (!monsterBase.aggroController.HasTarget())
                {
                    var obj = Runner.FindObject(ownerId);
                    if(!obj.gameObject) return;
                    if (obj.TryGetComponent(out AggroTarget target))
                    {
                        monsterBase.aggroController.ChangeAggroTarget(target);
                    }
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