namespace Util
{
    public class Systems : Singleton<Systems>
    {
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }
    }
}

