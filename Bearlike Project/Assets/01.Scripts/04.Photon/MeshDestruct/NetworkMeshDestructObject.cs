using System.Collections.Generic;
using UnityEngine;

namespace Photon
{
    public class NetworkMeshDestructObject : NetworkBehaviourEx
    {
        public static Dictionary<string, Material> MaterialDictionary = new Dictionary<string, Material>();
        public static Material GetMaterial(string matName) => MaterialDictionary.TryGetValue(matName, out var mat) ? mat : null;

        #region Unity Event Function
        private void Start()
        {
            var mat = GetComponent<MeshRenderer>().sharedMaterial;
            MaterialDictionary.TryAdd(mat.name, mat);
        }
        
        private void OnApplicationQuit()
        {
            MaterialDictionary.Clear();
        }
        
        #endregion
    }
}