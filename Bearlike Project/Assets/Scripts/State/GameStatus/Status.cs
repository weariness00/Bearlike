using UnityEngine;

namespace Scripts.State.GameStatus
{
    public class Status : MonoBehaviour
    {
        public Status[] statusArray;

        public StatusValue<float> hp = new StatusValue<float>();
        public StatusValue<float> mp = new StatusValue<float>();
        public StatusValue<float> damage = new StatusValue<float>();
        public StatusValue<float> speed = new StatusValue<float>();

        private void Start()
        {
            // 임시
            SetData();
            statusArray = new[]{ this };
        }

        public void SetData()
        {
            hp.Current = hp.Max = 100;
            damage.Current = damage.Max = 1;
            speed.Current = speed.Max = 10;
        }

        public void FindAllChildStatus()
        {
            statusArray = GetComponentsInChildren<Status>(true);
            foreach (var childStatus in statusArray)
            {
                childStatus.statusArray = statusArray;
            }
        }
    }
}

