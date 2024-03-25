using System;
using System.Collections.Generic;
using Manager;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UIElements;
using Util;

namespace Manager
{
    [System.Serializable]
    public enum KeyToAction
    {
        MoveFront = 0,
        MoveBack,
        MoveLeft,
        MoveRight,
        
        Jump,
        
        ItemInventory,
        SkillInventory,
        Interact,
        
        Attack,
        Reload,
        
        FirstSkill,
        Ultimate,
        
        LockCursor,
        Esc,
    }

    public struct KeyMapping
    {
        [JsonProperty("Action")] public string Action;
        [JsonProperty("Key")] public string Key;
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
        }

        private void Start()
        {
            DefaultLoad();
        }

        private void OnApplicationQuit()
        {
            Save();
        }
        
        #region Json Function

        public void Save()
        {
            Dictionary<KeyToAction, string> keyDictData = new Dictionary<KeyToAction, string>();
            foreach (var (key, value) in keyDictionary)
            {
                keyDictData.Add(key, value.ToString());
            }
            foreach (var (key, value) in mouseDictionary)
            {
                keyDictData.Add(key, value.ToString());
            }
            
            var data = JsonConvert.SerializeObject(keyDictData);
            
            JsonConvertExtension.Save(data, "KeyData");

            // File.WriteAllText(Application.dataPath + "/Json/KeyManager/KeyData.json", data);
        }

        public void Load(string fileName)
        {
            StartCoroutine(JsonConvertExtension.LoadCoroutine(fileName, (data) =>
            {
                var keyMapping = JsonConvert.DeserializeObject<KeyMapping[]>(data);

                keyDictionary.Clear();
                mouseDictionary.Clear();
                foreach (var mapping in keyMapping)
                {
                    var action = mapping.Action;
                    var key = mapping.Key;
                    if (Enum.TryParse(action, out KeyToAction keyToAction))
                    {
                        if (Enum.TryParse(key, out KeyCode keyCode))
                        {
                            keyDictionary.Add(keyToAction, keyCode);
                        }
                        else if (Enum.TryParse(key, out MouseButton mouseButton))
                        {
                            mouseDictionary.Add(keyToAction, mouseButton);
                        }
                    }
                }
            }));
        }

        void DefaultLoad() => Load("DefaultKeyData");

        #endregion

        #region Action Function

        public static bool InputActionDown(KeyToAction action)
        {
            if (KeyDictionary.TryGetValue(action, out var key) &&
                Input.GetKeyDown(key))
            {
                if(IsDebug){DebugManager.Log($"Key Down : {key}");}
                return true;
            }
            else if (MouseDictionary.TryGetValue(action, out var mouse) && 
                     Input.GetMouseButtonDown((int)mouse))
            {
                if(IsDebug){DebugManager.Log($"Mouse Down : {mouse}");}
                return true;
            }

            return false;
        }
        
        public static bool InputActionUp(KeyToAction action)
        {
            if (KeyDictionary.TryGetValue(action, out var key) &&
                Input.GetKeyUp(key))
            {
                if(IsDebug){DebugManager.Log($"Key Up : {key}");}
                return true;
            }
            else if (MouseDictionary.TryGetValue(action, out var mouse) && 
                     Input.GetMouseButtonUp((int)mouse))
            {
                if(IsDebug){DebugManager.Log($"Mouse Up : {mouse}");}
                return true;
            }

            return false;
        }

        public static bool InputAction(KeyToAction action)
        {
            if (KeyDictionary.TryGetValue(action, out var key) &&
                Input.GetKey(key))
            {
                if(IsDebug){DebugManager.Log($"Key Press : {key}");}
                return true;
            }
            else if (MouseDictionary.TryGetValue(action, out var mouse) && 
                     Input.GetMouseButton((int)mouse))
            {
                if(IsDebug){DebugManager.Log($"Mouse Press : {mouse}");}
                
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
        
        #endregion
    }
}