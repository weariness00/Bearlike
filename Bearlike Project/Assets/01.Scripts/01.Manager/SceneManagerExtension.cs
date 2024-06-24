using System;
using System.Collections.Generic;
using UnityEngine;
using Util;

namespace Manager
{
    public class SceneManagerExtension : MonoBehaviour
    {
        public List<GameObject> disableObjectList;

        public void Awake()
        {
            foreach (var o in disableObjectList)
            {
                o.SetActive(false);
            }
        }
    }
}

