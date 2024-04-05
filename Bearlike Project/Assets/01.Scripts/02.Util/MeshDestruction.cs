using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Manager;
using Parabox.CSG;
using Unity.VisualScripting;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Util
{
    public static class MeshDestruction
    {
        public static List<GameObject> Destruction(GameObject targetObject, PrimitiveType shapeType, Vector3 position, Vector3 size, bool isDestroyOrigin = true, params Type[] components)
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
                var sliceObjects = MeshSlicing.Slice(targetObject, Random.onUnitSphere.normalized, position, components);
                return sliceObjects.ToList();
            }

            var componentList = new List<Type>{ typeof(MeshFilter), typeof(MeshRenderer) };
            componentList.AddRange(components);
            components = componentList.ToArray();
            
            GameObject shapeObject = GameObject.CreatePrimitive(shapeType);
            shapeObject.transform.position = position;
            shapeObject.transform.localScale = size;
            
            // 겹치는 영역 만들기
            GameObject intersectObject = null;
            GameObject subtractObject = null;
            try
            {
                var intersectModel = CSG.Intersect(targetObject, shapeObject);
                intersectObject = CreateDestructionGameObject(targetObject, intersectModel, components);
                if(intersectObject.name.Contains("_Intersect") == false) intersectObject.name += "_Intersect";
            }
            catch (Exception e)
            {
                DebugManager.LogWarning($"Intersect가 실패했습니다." +
                                        $"{targetObject.name}와 충돌 {shapeType}의 영역이 충돌하지 않습니다.");

                if(intersectObject != null) Object.Destroy(intersectObject);
                Object.Destroy(shapeObject);
                return destructionObjects;
            }
            
            // 겹치는 영역제외하고 만들기
            try
            {
                var subtractModel = CSG.Subtract(targetObject, shapeObject);
                subtractObject = CreateDestructionGameObject(targetObject, subtractModel, components);
                
                if(subtractObject.name.Contains("_Subtract") == false) subtractObject.name += "_Subtract";
            }
            catch (Exception e)
            {
                DebugManager.LogWarning($"Subtract 실패했습니다." +
                                        $"{targetObject.name}와 충돌 {shapeType}의 영역이 충돌하지 않습니다.");

                Object.Destroy(intersectObject);
                Object.Destroy(shapeObject);
                return destructionObjects;
            }

            // 겹치는 영역 Slicing 하기
            try
            {
                destructionObjects.AddRange(MeshSlicing.Slice(intersectObject, Random.onUnitSphere.normalized, position, components));

                if (destructionObjects.Count == 1)
                {
                    destructionObjects.Clear();
                    destructionObjects.Add(intersectObject);
                }
            }
            catch (Exception e)
            {
                DebugManager.LogWarning($"Slicing에 실패했습니다.");

                if(intersectObject != null) Object.Destroy(intersectObject);
                Object.Destroy(subtractObject);
                Object.Destroy(shapeObject);
                foreach (var destructionObject in destructionObjects)
                {
                    Object.Destroy(destructionObject);
                }
                return destructionObjects;
            }

            destructionObjects.Add(subtractObject);
            
            if(isDestroyOrigin) Object.Destroy(targetObject);
            Object.Destroy(shapeObject);
            return destructionObjects;
        }

        private static GameObject CreateDestructionGameObject(GameObject targetObject, Model model, params Type[] components)
        {
            var name = targetObject.name;
            if (name.Contains("_Destruction") == false) name += "_Destruction";
            var obj = new GameObject(name, components);
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

