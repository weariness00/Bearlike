using System;
using UnityEngine;

namespace Script.GameStatus
{
    public class Status : MonoBehaviour
    {
        public StatusValue hp = new StatusValue();
        public StatusValue mp = new StatusValue();
        public StatusValue damage = new StatusValue();
        public StatusValue speed = new StatusValue();

        private void Start()
        {
            // 임시
            SetData();
        }

        public void SetData()
        {
            hp.Current = hp.Max = 100;
            damage.Current = damage.Max = 1;
            speed.Current = speed.Max = 10;
        }
    }
}

