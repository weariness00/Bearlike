using System;
using System.Collections.Generic;
using UnityEngine;

namespace Script.GameStatus
{
    public class Status : MonoBehaviour
    {
        public Status[] statusArray;

        public StatusValue hp = new StatusValue();
        public StatusValue mp = new StatusValue();
        public StatusValue damage = new StatusValue();
        public StatusValue speed = new StatusValue();

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

