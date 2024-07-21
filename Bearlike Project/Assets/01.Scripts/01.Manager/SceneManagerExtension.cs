using System;
using System.Collections.Generic;
using UnityEngine;
using Util;

namespace Manager
{
    public class SceneManagerExtension : MonoBehaviour
    {
        public List<GameObject> disableObjectList;
        public List<GameObject> activeFalseObjectList;
        public List<GameObject> activeTrueObjectList;
        public List<GameObject> dontDestroyObjectList;
        
        public virtual void Awake()
        {
            foreach (var o in dontDestroyObjectList)
            {
                DontDestroyOnLoad(o);
            }
            
            foreach (var o in disableObjectList)
            {
                Destroy(o);
            }

            foreach (var o in activeFalseObjectList)
            {
                o.SetActive(false);
            }
            
            foreach (var o in activeTrueObjectList)
            {
                o.SetActive(true);
            }
        }
    }
}

