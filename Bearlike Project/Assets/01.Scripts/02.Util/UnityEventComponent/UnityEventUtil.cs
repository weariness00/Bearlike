using System;
using UnityEngine;

namespace Util.UnityEventComponent
{
    public class UnityEventUtil : MonoBehaviour
    {
        public void AddOnTriggerEnter(Action<Collider> triggerEnterAction)
        {
            if (gameObject.TryGetComponent(out OnTriggerEnterUtil util) == false)
                util = gameObject.AddComponent<OnTriggerEnterUtil>();
            
            util.onTriggerEnterAction += triggerEnterAction;
        }

        public void RemoveOnTriggerEnter(Action<Collider> triggerEnterAction)
        {
            if (gameObject.TryGetComponent(out OnTriggerEnterUtil util))
            {
                util.onTriggerEnterAction -= triggerEnterAction;
                if(util.onTriggerEnterAction == null) DestroyImmediate(util);
            }
        }
    }
}

