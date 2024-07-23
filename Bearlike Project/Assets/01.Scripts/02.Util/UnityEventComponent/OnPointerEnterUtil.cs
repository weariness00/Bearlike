using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Util.UnityEventComponent
{
    public class OnPointerEnterUtil : MonoBehaviour, IUnityEventUtil, IPointerEnterHandler
    {
        private Action<PointerEventData> onPointerEnterAction;
        
        public bool IsHasAction => onPointerEnterAction != null;

        public void AddAction<T>(Action<T> action)
        {
            if(action is Action<PointerEventData> a)
                onPointerEnterAction += a;
        }
        
        public void RemoveAction<T>(Action<T> action)
        {
            if(action is Action<PointerEventData> a)
                onPointerEnterAction -= a;
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            onPointerEnterAction?.Invoke(eventData);
        }
    }
}

