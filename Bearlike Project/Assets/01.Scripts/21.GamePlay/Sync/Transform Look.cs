using System;
using Status;
using UnityEngine;

namespace GamePlay.Sync
{
    public class TransformLook : MonoBehaviour
    {
        [Range(0f, 1f)] public float weight;

        public Transform targetTransform;
        public StatusValue<float> range = new StatusValue<float>(){Min = -30, Max = 30};

        public void LateUpdate()
        {
            var angle =  weight * targetTransform.eulerAngles;
            angle.x = Mathf.Clamp(angle.x, range.Min, range.Max); // 위, 아래 제한 
            angle.y = Mathf.Clamp(angle.y, range.Min, range.Max); // 위, 아래 제한 
            angle.z = Mathf.Clamp(angle.z, range.Min, range.Max); // 위, 아래 제한 
            transform.eulerAngles = angle;
        }
    }
}

