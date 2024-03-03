using System;
using System.Collections.Generic;
using System.Linq;
using Parabox.CSG;
using Script.Manager;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Util
{
    public static class MeshDestruction
    {
        public static List<GameObject> Destruction(GameObject targetObject, PrimitiveType shapeType, Vector3 position , Vector3 size, bool isTargetDestroy = true)
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
            if ((max - min).magnitude < 1f) return destructionObjects;
            
            GameObject shapeObject = GameObject.CreatePrimitive(shapeType);
            shapeObject.transform.position = position;
            shapeObject.transform.localScale = size;

            var name = targetObject.name;
            if (name.Contains("_Destruction") == false) name += "_Destruction";
            List<Model> modelList = new List<Model>();
            // 겹치는 영역 만들기
            try
            {
                var intersectModel = CSG.Intersect(targetObject, shapeObject);
                modelList.Add(intersectModel);
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
                modelList.Add(subtractModel);
            }
            catch (Exception e)
            {
                DebugManager.LogWarning($"Subtract 실패했습니다." +
                                        $"{targetObject.name}와 충돌 {shapeType}의 영역이 충돌하지 않습니다.");
            }

            // Object 만들기
            foreach (var model in modelList)
            {
                var obj = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
                var centerVertex = new Vector3(
                    model.mesh.vertices.Average(vertex => vertex.x),
                    model.mesh.vertices.Average(vertex => vertex.y),
                    model.mesh.vertices.Average(vertex => vertex.z)
                );
                var vertices = model.mesh.vertices;
                for (var i = 0; i < vertices.Length; i++)
                    vertices[i] -= centerVertex;
                model.mesh.vertices = vertices;
                
                // Object 생성
                obj.GetComponent<MeshFilter>().sharedMesh = model.mesh;
                obj.GetComponent<MeshRenderer>().sharedMaterials = model.materials.ToArray();
                
                obj.AddComponent<MeshCollider>();
                var rigid = obj.AddComponent<Rigidbody>();
                
                obj.transform.SetParent(targetObject.transform.parent);
                obj.tag = "Destruction";
                
                destructionObjects.Add(obj);
            }

            if (isTargetDestroy)
            {
                Object.Destroy(targetObject);
            }
            Object.Destroy(shapeObject);
            return destructionObjects;
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

