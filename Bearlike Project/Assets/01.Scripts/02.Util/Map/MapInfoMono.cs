using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Util.Map
{
    [System.Serializable]
    public struct MapInfo
    {
        public Vector3 pivot;
        public Vector3 size;
    }
    
    public class MapInfoMono : MonoBehaviour
    {
        public MapInfo info;
    }
}

