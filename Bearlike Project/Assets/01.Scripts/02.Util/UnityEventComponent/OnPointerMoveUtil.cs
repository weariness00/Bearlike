using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Util.UnityEventComponent;

namespace Util.UnityEventComponent
{
    public class OnPointerMoveUtil : MonoBehaviour, IUnityEventUtil, IPointerMoveHandler
    {
        private Action<PointerEventData> onPointerMoveAction;

        public bool IsHasAction => onPointerMoveAction != null;
        public void AddAction<T>(Action<T> action)
        {
            if(action is Action<PointerEventData> a)
                onPointerMoveAction += a;
        }

        public void RemoveAction<T>(Action<T> action)
        {
            if(action is Action<PointerEventData> a)
                onPointerMoveAction -= a;
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            onPointerMoveAction?.Invoke(eventData);
        }
    }
}

