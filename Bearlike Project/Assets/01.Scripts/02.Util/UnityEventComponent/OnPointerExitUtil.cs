using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Util.UnityEventComponent;

namespace Util.UnityEventComponent
{
    public class OnPointerExitUtil : MonoBehaviour, IUnityEventUtil, IPointerExitHandler
    {
        private Action<PointerEventData> onPointerExitAction;

        public bool IsHasAction => onPointerExitAction != null;
        public void AddAction<T>(Action<T> action)
        {
            if (action is Action<PointerEventData> a)
                onPointerExitAction += a;
        }

        public void RemoveAction<T>(Action<T> action)
        {
            if (action is Action<PointerEventData> a)
                onPointerExitAction -= a;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            onPointerExitAction?.Invoke(eventData);
        }
    }
}
