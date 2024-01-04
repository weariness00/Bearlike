using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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
    }
    
    public class KeyManager : MonoBehaviour
    {
        public static KeyManager Instance;
        public Dictionary<KeyToAction, KeyCode> keyDictionary = new Dictionary<KeyToAction, KeyCode>();

        private void Awake()
        { 
            if(Instance == null) Instance = this;
        }

        public bool InputActionDown(KeyToAction action)
        {
            if (!keyDictionary.ContainsKey(action))
                return false;

            return Input.GetKeyDown(keyDictionary[action]);
        }

        public bool InputAction(KeyToAction action)
        {
            if (!keyDictionary.ContainsKey(action))
                return false;

            return Input.GetKey(keyDictionary[action]);
        }

        public bool InputAnyKey
        {
            get
            {
                foreach (KeyCode key in keyDictionary.Values)
                {
                    if (Input.GetKey(key))
                        return true;
                }

                return false;
            }
        }
    }
}