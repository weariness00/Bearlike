using System;
using UnityEngine;

namespace Util.UnityEventComponent
{
    public class OnTriggerEnterUtil : MonoBehaviour, IUnityEventUtil
    {
        public Action<Collider> onTriggerEnterAction;

        public bool IsHasAction => onTriggerEnterAction != null;
        public void AddAction<T>(Action<T> action)
        {
            if(action is Action<Collider> a)
                onTriggerEnterAction += a;
        }

        public void RemoveAction<T>(Action<T> action)
        {
            if(action is Action<Collider> a)
                onTriggerEnterAction -= a;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            onTriggerEnterAction?.Invoke(other);
        }
    }
}

