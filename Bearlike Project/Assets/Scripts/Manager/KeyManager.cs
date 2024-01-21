using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Script.Manager
{
    [System.Serializable]
    public class KeyDataInfo
    {
        public KeyCode Key;
        public KeyToAction Action;
    }

    [System.Serializable]
    public enum KeyToAction
    {
        MoveFront = 0,
        MoveBack,
        MoveLeft,
        MoveRight,
        
        Attack,
        ReLoad,
        
        Esc,
    }
    
    public class KeyManager : MonoBehaviour
    {
        public static KeyManager Instance;
        private static bool IsDebug => KeyManager.Instance.isDebug;
        private static Dictionary<KeyToAction, KeyCode> KeyDictionary => KeyManager.Instance.keyDictionary;
        private static Dictionary<KeyToAction, MouseButton> MouseDictionary => KeyManager.Instance.mouseDictionary;
        
        Dictionary<KeyToAction, KeyCode> keyDictionary = new Dictionary<KeyToAction, KeyCode>();
        Dictionary<KeyToAction, MouseButton> mouseDictionary = new Dictionary<KeyToAction, MouseButton>();
        public bool isDebug;

        private void Awake()
        { 
            if(Instance == null) Instance = this;
            DefaultKeySetting();
        }
        
        public void DefaultKeySetting()
        {
            keyDictionary.Add(KeyToAction.MoveFront, KeyCode.W);
            keyDictionary.Add(KeyToAction.Esc, KeyCode.Escape);
            keyDictionary.Add(KeyToAction.ReLoad, KeyCode.R);
            
            mouseDictionary.Add(KeyToAction.Attack, MouseButton.LeftMouse);
        }

        public static bool InputActionDown(KeyToAction action)
        {
            if (KeyDictionary.TryGetValue(action, out var key) &&
                Input.GetKeyDown(key))
            {
                DebugManager.Log($"Key Down : {key}");
                return true;
            }
            else if (MouseDictionary.TryGetValue(action, out var mouse) && 
                     Input.GetMouseButtonDown((int)mouse))
            {
                DebugManager.Log($"Mouse Down : {mouse}");
                return true;
            }

            return false;
        }

        public static bool InputAction(KeyToAction action)
        {
            if (KeyDictionary.TryGetValue(action, out var key) &&
                Input.GetKey(key))
            {
                DebugManager.Log($"Key Press : {key}");
                return true;
            }
            else if (MouseDictionary.TryGetValue(action, out var mouse) && 
                     Input.GetMouseButton((int)mouse))
            {
                DebugManager.Log($"Mouse Press : {mouse}");
                return true;
            }

            return false;
        }

        // 행동을 Key로 변경
        public static void ChangeAction(KeyToAction action, KeyCode code)
        {
            // 이미 쓰고 있는 Key이면 제거
            if (KeyDictionary.ContainsValue(code))
            {
                foreach (var (key, value) in KeyDictionary)
                {
                    if (value == code)
                    {
                        KeyDictionary.Remove(key);
                        break;
                    }
                }
            }
            // Mouse 중에 사용하는 것이면 제거
            else if (MouseDictionary.ContainsKey(action))
                MouseDictionary.Remove(action);

            KeyDictionary[action] = code;
        }
        
        // 행동을 Mouse Button 으로 변경
        public static void ChangeAction(KeyToAction action, MouseButton button)
        {
            // 이미 쓰고 있는 Mouse이면 제거
            if (MouseDictionary.ContainsValue(button))
            {
                foreach (var (key, value) in MouseDictionary)
                {
                    if (value == button)
                    {
                        MouseDictionary.Remove(key);
                        break;
                    }
                }
            }
            // Key 중에 사용하는 것이면 제거
            else if (KeyDictionary.ContainsKey(action))
                MouseDictionary.Remove(action);

            MouseDictionary[action] = button;
        }
    }
}