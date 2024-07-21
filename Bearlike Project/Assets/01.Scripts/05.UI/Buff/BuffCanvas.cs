using System;
using System.Collections.Generic;
using Manager;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace UI
{
    public class BuffCanvas : MonoBehaviour
    {
        [SerializeField] private Transform parent;
        [SerializeField] private GameObject uiObject;

        private Dictionary<string, BuffBlockHandle> _buffUIList = new Dictionary<string, BuffBlockHandle>();

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        public void SpawnUI(string _name)
        {
            var obj = Instantiate(uiObject, parent);
            var blockHandle = obj.GetComponent<BuffBlockHandle>();
            obj.SetActive(true);
            _buffUIList.Add(_name, blockHandle);
        }

        public void RemoveUI(string _name)
        {
            if (_buffUIList.TryGetValue(_name, out var blockHandle))
            {
                Destroy(blockHandle.gameObject);
                _buffUIList.Remove(_name);
            }
            else
            {
                DebugManager.LogWarning($"[{_name}] 이라는 Buff UI가 존재하지 않습니다.");
            }
        }

        public bool HasUI(string _name) => _buffUIList.ContainsKey(_name);

        public void SetIcon(string _name, Sprite sprite)
        {
            if (_buffUIList.TryGetValue(_name, out var uiStruct))
            {
                uiStruct.icon.sprite = sprite;
            }
        }

        public void SetStackText(string _name, int stack)
        {
            if (_buffUIList.TryGetValue(_name, out var uiStruct))
            {
                uiStruct.stackText.text = stack.ToString();
            }
        }

        public void SetTimer(string _name, float value)
        {
            if (_buffUIList.TryGetValue(_name, out var uiStruct))
            {
                uiStruct.SetTimer(value);
            }
        }
    }
}