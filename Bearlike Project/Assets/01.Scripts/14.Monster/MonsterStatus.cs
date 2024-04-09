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

        #region Json Data Interfacec

        public override void SetJsonData(StatusJsonData json)
        {
            base.SetJsonData(json);
        }

        #endregion
    }
}