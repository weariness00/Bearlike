using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Manager;
using Parabox.CSG;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Util
{
    public static class MeshDestruction
    {
        public static List<GameObject> Destruction(GameObject targetObject, PrimitiveType shapeType, Vector3 position, Vector3 size, Vector3 force)
        {
            var targetMeshFilter = targetObject.GetComponent<MeshFilter>();
            List<GameObject> destructionObjects = new List<GameObject>();
            if (targetMeshFilter == null)
            {
                DebugManager.LogError($"{targetObject.name} 에 Mesh Filter가 존재하지 않아 Destruction을 진행 할 수 없습니다.");
                return destructionObjects;
            }

            // 메쉬의 크기가 크지 않을 경우는 무시
            var min = VectorMultiple(targetMeshFilter.sharedMesh.bounds.min, targetObject.transform.localScale);
            var max = VectorMultiple(targetMeshFilter.sharedMesh.bounds.max, targetObject.transform.localScale);
            if ((max - min).magnitude < size.magnitude) return destructionObjects;
            if ((max - min).magnitude < 5f) // 일정 크기 이하면 slice 하기
            {
                var sliceObjects = MeshSlicing.Slice(targetObject, Random.onUnitSphere.normalized, position);
                return sliceObjects.ToList();
            }
            
            GameObject shapeObject = GameObject.CreatePrimitive(shapeType);
            shapeObject.transform.position = position;
            shapeObject.transform.localScale = size;
            
            // 겹치는 영역 만들기
            try
            {
                var intersectModel = CSG.Intersect(targetObject, shapeObject);
                var obj = CreateDestructionGameObject(targetObject, intersectModel);
                var sliceObjects = MeshSlicing.Slice(obj, Random.onUnitSphere.normalized, position);
                if (sliceObjects != Array.Empty<GameObject>())
                {
                    destructionObjects.AddRange(MeshSlicing.Slice(sliceObjects[0], Random.onUnitSphere.normalized, position));
                    destructionObjects.AddRange(MeshSlicing.Slice(sliceObjects[1], Random.onUnitSphere.normalized, position));
                }
                else
                    destructionObjects.AddRange(sliceObjects);
                
                // foreach (var destructionObject in destructionObjects)
                // {
                //     var rigid = destructionObject.GetComponent<Rigidbody>();
                //     rigid.AddForce(force);
                // }
            }
            catch (Exception e)
            {
                DebugManager.LogWarning($"Intersect가 실패했습니다." +
                                        $"{targetObject.name}와 충돌 {shapeType}의 영역이 충돌하지 않습니다.");
            }
            
            // 겹치는 영역제외하고 만들기
            try
            {
                var subtractModel = CSG.Subtract(targetObject, shapeObject);
                var obj = CreateDestructionGameObject(targetObject, subtractModel);
                
                destructionObjects.Add(obj);
            }
            catch (Exception e)
            {
                DebugManager.LogWarning($"Subtract 실패했습니다." +
                                        $"{targetObject.name}와 충돌 {shapeType}의 영역이 충돌하지 않습니다.");
                foreach (var destructionObject in destructionObjects)
                {
                    Object.Destroy(destructionObject);
                }
                destructionObjects.Clear();
                Object.Destroy(shapeObject);
                return destructionObjects;
            }
            
            Object.Destroy(targetObject);
            Object.Destroy(shapeObject);
            return destructionObjects;
        }

        private static GameObject CreateDestructionGameObject(GameObject targetObject, Model model)
        {
            var name = targetObject.name;
            if (name.Contains("_Destruction") == false) name += "_Destruction";
            var obj = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer),typeof(MeshCollider));
            var copyMesh = model.mesh;
            var centerVertex = new Vector3( 
                copyMesh.vertices.Average(vertex => vertex.x),
                copyMesh.vertices.Average(vertex => vertex.y),
                copyMesh.vertices.Average(vertex => vertex.z)
            );
            var vertices = copyMesh.vertices;
            for (var i = 0; i < vertices.Length; i++)
                vertices[i] -= centerVertex;
            copyMesh.SetVertices(vertices);
            copyMesh.RecalculateNormals();
            copyMesh.RecalculateBounds();
                
            // Object 생성
            obj.GetComponent<MeshFilter>().sharedMesh = copyMesh;
            obj.GetComponent<MeshRenderer>().sharedMaterials = model.materials.ToArray();
                
            var collider = obj.GetComponent<MeshCollider>();
            // var rigid = obj.GetComponent<Rigidbody>();

            collider.convex = true;
            collider.sharedMesh = copyMesh;

            obj.tag = "Destruction";
            obj.transform.position += centerVertex;
            if(targetObject.transform.parent) obj.transform.SetParent(targetObject.transform.parent);

            return obj;
        }

        private static Vector3 VectorMultiple(Vector3 a, Vector3 b)
        {
            Vector3 v = Vector3.zero;
            v.x = a.x * b.x;
            v.y = a.y * b.y;
            v.z = a.z * b.z;
            return v;
        }
    }
}

