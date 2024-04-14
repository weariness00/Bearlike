using Fusion;
using Player;

namespace Status
{
    /// <summary>
    /// Monster의 State을 나타내는 Class
    /// </summary>
    public class MonsterStatus : StatusBase
    {
        private void Start()
        {
            InvokeRepeating(nameof(MainLoop), 0.0f, 1.0f);
        }

        #region Member Function
        
        public override void MainLoop()
        {
            if (PoisonedIsOn())
            {
                BePoisoned(Define.PoisonDamage);
                ShowInfo();
            }
        }
        
        public void BePoisoned(int value)
        {
            hp.Current -= value;
        }

        public override void ApplyDamage(int applyDamage, NetworkId ownerId, CrowdControl cc)
        {
            base.ApplyDamage(applyDamage, ownerId, cc);
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

        #endregion

        #region Json Data Interfacec

        public override void SetJsonData(StatusJsonData json)
        {
            base.SetJsonData(json);
        }

        #endregion
    }
}