using System;
using UnityEngine;

namespace Util.UnityEventComponent
{
    public class OnTriggerEnterUtil : MonoBehaviour
    {
        public Action<Collider> onTriggerEnterAction;

        private void OnTriggerEnter(Collider other)
        {
            onTriggerEnterAction?.Invoke(other);
        }
    }
}

