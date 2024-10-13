using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

namespace Util.UnityEventComponent
{
    public interface IUnityEventUtil
    {
        public bool IsHasAction { get;}
        public void AddAction<T>(Action<T> action);
        public void RemoveAction<T>(Action<T> action);
    }
    
    public static class UnityEventUtil
    {
        private static void AddEvent<TEvent, TAction>(GameObject gameObject, Action<TAction> action) where TEvent : Component
        {
            if (gameObject.TryGetComponent(out TEvent util) == false)
                util = gameObject.AddComponent<TEvent>();
            
            if(util is IUnityEventUtil eventUtil)
                eventUtil.AddAction(action);
        }

        private static void RemoveEvent<TEvent, TAction>(GameObject gameObject, Action<TAction> action) where TEvent : Component
        {
            if (gameObject.TryGetComponent(out TEvent util))
            {
                if (util is IUnityEventUtil eventUtil)
                {
                    eventUtil.RemoveAction(action);
                    if(eventUtil.IsHasAction == false) Object.DestroyImmediate(util);
                }
            }
        }

        // Trigger Event
        public static void AddOnTriggerEnter(this GameObject gameObject, Action<Collider> action) => AddEvent<OnTriggerEnterUtil, Collider>(gameObject, action);
        public static void RemoveOnTriggerEnter(this GameObject gameObject, Action<Collider> action) => RemoveEvent<OnTriggerEnterUtil, Collider>(gameObject, action);

        // Pointer Event
        public static void AddOnPointerEnter(this GameObject gameObject, Action<PointerEventData> action) => AddEvent<OnPointerEnterUtil, PointerEventData>(gameObject, action);
        public static void RemoveOnPointerEnter(this GameObject gameObject, Action<PointerEventData> action) => RemoveEvent<OnPointerEnterUtil, PointerEventData>(gameObject, action);
        
        public static void AddOnPointerMove(this GameObject gameObject, Action<PointerEventData> action) => AddEvent<OnPointerMoveUtil, PointerEventData>(gameObject, action);
        public static void RemoveOnPointerMove(this GameObject gameObject, Action<PointerEventData> action) => RemoveEvent<OnPointerMoveUtil, PointerEventData>(gameObject, action);
        
        public static void AddOnPointerExit(this GameObject gameObject, Action<PointerEventData> action) => AddEvent<OnPointerExitUtil, PointerEventData>(gameObject, action);
        public static void RemoveOnPointerExit(this GameObject gameObject, Action<PointerEventData> action) => RemoveEvent<OnPointerExitUtil, PointerEventData>(gameObject, action);
    }
}

