using System;
using Fusion;
using Manager;
using Photon;
using Test;
using Unity.VisualScripting;
using UnityEngine;
using Util;
using Object = UnityEngine.Object;

namespace _00.Scenes.Test___Dong_Woo__
{
    public class TestMeshDestruction : NetworkBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        public override void FixedUpdateNetwork()
        {
            if (HasStateAuthority == false)
            {
                return;      
            }
            
            if (GetInput(out TsetServer.TestInputData data))
            {
                if (data.Click)
                {
                    Vector3 detination = Vector3.zero;
                    Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
                    var hitOptions = HitOptions.IncludePhysX | HitOptions.IgnoreInputAuthority;
                    DebugManager.DrawRay(ray.origin, ray.direction * int.MaxValue, Color.red, 1.0f);
                    if(Runner.LagCompensation.Raycast(ray.origin, ray.direction, float.MaxValue, Object.InputAuthority, out var hit, Int32.MaxValue, hitOptions))
                    {
                        DebugManager.Log($"Ray충돌\n총 이름 : {name}\n맞은 대상 : {hit.GameObject.name}");
                
                        var hitbox = hit.Hitbox;
                        if (hitbox == null)
                        {
                            if (hit.GameObject.CompareTag("Destruction"))
                            {
                                // NetworkMeshDestructSystem.Instance.DestructRPC(hit.GameObject.GetComponent<NetworkObject>().Id,PrimitiveType.Cube, hit.Point, Vector3.one * 2, ray.direction);
                                // MeshDestruction.Destruction(hit.GameObject, PrimitiveType.Cube, hit.Point, Vector3.one * 2, ray.direction);
                            }
                        }   

                        detination = hit.Point;
                    }
                }
            }
        }
    }
}
