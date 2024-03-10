using System;
using System.Collections.Generic;
using Manager;
using Unity.VisualScripting;
using UnityEngine;
using Util;

namespace _00.Scenes.Test___Dong_Woo__
{
    public class Test2 : MonoBehaviour
    {
        private void Start()
        {
            
        }

        private void Update()
        {
            if (Input.GetMouseButton((int)MouseButton.Left))
            {
                Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
                if(Physics.Raycast(ray.origin, ray.direction, out var hit,float.MaxValue ))
                {
                    DebugManager.Log($"Ray충돌\n총 이름 : {name}\n맞은 대상 : {hit.transform.gameObject.name}");
                
                    if (hit.collider.CompareTag("Destruction"))
                    {
                        MeshDestruction.Destruction(hit.collider.gameObject, PrimitiveType.Cube, hit.point, Vector3.one * 2, ray.direction);
                    }
                }
            }
        }
    }
}