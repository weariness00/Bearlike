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
        public static void Subscribe(EventBusType eventType, Action action) => Instance.SubscribeAction(eventType, action);
        public static void Subscribe<T>(EventBusType eventType, Action<T> action) => Instance.SubscribeAction(eventType, action);
        public static void UnSubscribe(EventBusType eventType, Action action) => Instance.UnSubscribeAction(eventType, action);
        public static void UnSubscribe<T>(EventBusType eventType, Action<T> action) => Instance.UnSubscribeAction(eventType, action);
        public static void Publish(EventBusType eventType) => Instance.PublishAction(eventType);
        public static void Publish<T>(EventBusType eventType, T argument) => Instance.PublishAction(eventType, argument);
        
        private readonly IDictionary<EventBusType, Delegate> actionTemplateDictionary = new Dictionary<EventBusType, Delegate>();
        private readonly IDictionary<EventBusType, Action> actionDictionary = new Dictionary<EventBusType, Action>();

        void SubscribeAction(EventBusType key, Action action)
        {
            if (actionDictionary.TryGetValue(key, out Action existingAction))
            {
                actionDictionary[key] = existingAction + action;
            }
            else
            {
                actionDictionary[key] = action;
            }
        }
        
        void SubscribeAction<T>(EventBusType key, Action<T> action)
        {
            if (actionTemplateDictionary.TryGetValue(key, out Delegate existingDelegate))
            {
                actionTemplateDictionary[key] = Delegate.Combine(existingDelegate, action);
            }
            else
            {
                actionTemplateDictionary[key] = action;
            }
        }

        void UnSubscribeAction(EventBusType key, Action action)
        {
            if (actionDictionary.TryGetValue(key, out Action existingAction))
            {
                actionDictionary[key] = existingAction - action;
            }
        }
        
        void UnSubscribeAction<T>(EventBusType key, Action<T> action)
        {
            if (actionTemplateDictionary.TryGetValue(key, out Delegate existingDelegate))
            {
                actionTemplateDictionary[key] = Delegate.Remove(existingDelegate, action);
            }
        }
        
        void PublishAction(EventBusType key)
        {
            if (actionDictionary.TryGetValue(key, out Action action))
            {
                action.Invoke();
            }
            else
            {
                DebugManager.LogWarning($"{key}에 아무런 Event가 없습니다.");
            }
        }
        
        void PublishAction<T>(EventBusType key, T argument)
        {
            if (actionTemplateDictionary.TryGetValue(key, out Delegate action))
            {
                if (action is Action<T> typedAction)
                {
                    typedAction.Invoke(argument);
                }
                else
                {
                    DebugManager.LogWarning($"Event Bus {key}에서 Type[{typeof(T)}]에 해당하는 Event가 없습니다.");
                }
            }
            else
            {
                DebugManager.LogWarning($"{key}에 아무런 Event가 없습니다.");
            }
        }
    }
}

