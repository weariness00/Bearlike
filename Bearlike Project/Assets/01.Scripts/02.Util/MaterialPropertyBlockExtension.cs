using System;
using Manager;
using UnityEngine;

namespace Util
{
    public class MaterialPropertyBlockExtension : MonoBehaviour
    {
        [SerializeField] private new Renderer renderer; 
        [SerializeField] private Material material;
        private MaterialPropertyBlock _block;
        private int index;

        public MaterialPropertyBlock Block => _block;

        private void Awake()
        {
            Init();
        }

        public void Init()
        {
            _block = new MaterialPropertyBlock();
            foreach (var mat in renderer.materials)
            {
                string matName = mat.name.Replace("(Instance)", "").Trim();
                if (matName == material.name)
                {
                    renderer.GetPropertyBlock(_block, index);
                    return;
                }
                ++index;
            }

            index = -1;
            DebugManager.LogError($"{material.name}이 해당 Renderer에 존재하지 않습니다.\n[Object : {GetInstanceID()}", gameObject);
        }

        public void SetBlock() => renderer.SetPropertyBlock(_block, index);
    }
}