using System;
using System.Collections.Generic;
using Util;

namespace Manager
{
    public enum EventBusType
    {
        AddSkill,
        SkillLevelUp,
    }
    
    public class EventBusManager : Singleton<EventBusManager>
    {
        public static void Subscribe<T>(EventBusType eventType, Action<T> action) => Instance.SubscribeAction(eventType, action);
        public static void UnSubscribe<T>(EventBusType eventType, Action<T> action) => Instance.UnSubscribeAction(eventType, action);
        public static void Publish<T>(EventBusType eventType, T argument) => Instance.PublishAction(eventType, argument);
        
        private readonly IDictionary<EventBusType, Delegate> actionDictionary = new Dictionary<EventBusType, Delegate>();
        
        void SubscribeAction<T>(EventBusType key, Action<T> action)
        {
            if (actionDictionary.TryGetValue(key, out Delegate existingDelegate))
            {
                actionDictionary[key] = Delegate.Combine(existingDelegate, action);
            }
            else
            {
                actionDictionary[key] = action;
            }
        }

        void UnSubscribeAction<T>(EventBusType key, Action<T> action)
        {
            if (actionDictionary.TryGetValue(key, out Delegate existingDelegate))
            {
                actionDictionary[key] = Delegate.Remove(existingDelegate, action);
            }
        }
        
        void PublishAction<T>(EventBusType key, T argument)
        {
            if (actionDictionary.TryGetValue(key, out Delegate action))
            {
                if (action is Action<T> typedAction)
                {
                    typedAction.Invoke(argument);
                }
                else
                {
                    DebugManager.LogWarning($"Event Bus {key}에 타잎[{typeof(T)}]에 해당하는 Event가 없습니다.");
                }
            }
            else
            {
                DebugManager.LogWarning($"{key}에 아무런 Event가 없습니다.");
            }
        }
    }
}

