using System.Collections.Generic;
using UnityEngine;

namespace _00.Scenes.Test___Dong_Woo__
{
    public class Test2 : MonoBehaviour
    {
        public GameObject polygon, hole;

        List<Vector3> polygonPos = new List<Vector3>(); /* 시계 방향 */
        List<Vector3> holePos = new List<Vector3>(); /* 반시계 방향 */

        List<Vector3> mergePolygon = new List<Vector3>();

        #region Polygon Triangulation

        public int numOfTriangle;
        List<Vector3> dotList = new List<Vector3>();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        Dictionary<Vector3, int> dic = new Dictionary<Vector3, int>();

        LineRenderer[] lineForTriangles;

        float CCWby2D(Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 p = b - a;
            Vector3 q = c - b;

            return Vector3.Cross(p, q).y;
        }

        void makeTriangle(LineRenderer lr, Vector3 a, Vector3 b, Vector3 c)
        {
            lr.startWidth = lr.endWidth = 0.1f;
            lr.material.color = Color.red;

            lr.positionCount = 3;

            lr.SetPosition(0, a);
            lr.SetPosition(1, b);
            lr.SetPosition(2, c);

            lr.loop = true;
        }

        float getAreaOfTriangle(Vector3 dot1, Vector3 dot2, Vector3 dot3)
        {
            Vector3 a = dot2 - dot1;
            Vector3 b = dot3 - dot1;
            Vector3 cross = Vector3.Cross(a, b);

            return cross.magnitude / 2.0f;
        }

        bool checkTriangleInPoint(Vector3 dot1, Vector3 dot2, Vector3 dot3, Vector3 checkPoint)
        {
            if (dot1 == checkPoint) return false;
            if (dot2 == checkPoint) return false;
            if (dot3 == checkPoint) return false;

            float area = getAreaOfTriangle(dot1, dot2, dot3);
            float dot12 = getAreaOfTriangle(dot1, dot2, checkPoint);
            float dot23 = getAreaOfTriangle(dot2, dot3, checkPoint);
            float dot31 = getAreaOfTriangle(dot3, dot1, checkPoint);

            return (dot12 + dot23 + dot31) <= area + 0.1f /* 오차 허용 */;
        }

        bool CrossCheckAll(List<Vector3> list, int index)
        {
            Vector3 a = list[index];
            Vector3 b = list[index + 1];
            Vector3 c = list[index + 2];

            for (int i = index + 3; i < list.Count; i++)
            {
                if (checkTriangleInPoint(a, b, c, list[i]) == true) return true;
            }

            return false;
        }

        void triangluation(int count)
        {
            vertices.Clear();
            triangles.Clear();
            dic.Clear();

            dotList.Clear();
            foreach (Vector3 v in mergePolygon) // init
                dotList.Add(v);

            lineForTriangles = this.GetComponentsInChildren<LineRenderer>();

            for (int i = 0; i < lineForTriangles.Length; i++) // init
                lineForTriangles[i].positionCount = 0;

            //int numOfTriangle = dotList.Count - 2;
            if (count > dotList.Count - 2) count = dotList.Count - 2;

            for (int i = 0; i < count; i++)
            {
                List<Vector3> copy = new List<Vector3>(dotList);

                for (int k = 0; k < copy.Count - 2; k++)
                {
                    bool ccw = (CCWby2D(copy[k], copy[k + 1], copy[k + 2]) > 0);
                    bool cross = CrossCheckAll(copy, k);

                    if (ccw == true && cross == false)
                    {
                        makeTriangle(lineForTriangles[i], copy[k], copy[k + 1], copy[k + 2]);

                        for (int c = 0; c < 3; c++)
                        {
                            if (dic.ContainsKey(copy[k + c])) continue;

                            dic[copy[k + c]] = vertices.Count;
                            vertices.Add(copy[k + c]);
                        }

                        for (int c = 0; c < 3; c++)
                            triangles.Add(dic[copy[k + c]]);

                        copy.RemoveAt(k + 1);
                        dotList = new List<Vector3>(copy);

                        break;
                    }
                }
            }
        }

        #endregion

        Vector3[] getShortestDots(List<Vector3> a, List<Vector3> b)
        {
            float minValue = float.MaxValue;
            Vector3 dot1, dot2;

            dot1 = dot2 = Vector3.zero;
            foreach (Vector3 v1 in a)
            {
                foreach (Vector3 v2 in b)
                {
                    float distance = Vector3.Distance(v1, v2);
                    if (distance < minValue)
                    {
                        minValue = distance;
                        dot1 = v1;
                        dot2 = v2;
                    }
                }
            }

            return new Vector3[] { dot1, dot2 };
        }

        List<Vector3> sortListbyStartPoint(List<Vector3> list, Vector3 startPos)
        {
            List<Vector3> sort = new List<Vector3>();
            List<Vector3> doubleList = new List<Vector3>();
            int count = list.Count;

            foreach (Vector3 v in list) doubleList.Add(v);
            foreach (Vector3 v in list) doubleList.Add(v);

            int start;
            for (start = 0; start < list.Count; start++)
                if (list[start] == startPos)
                    break;

            for (int i = start; i < start + count; i++) sort.Add(doubleList[i]);

            return sort;
        }

        List<Vector3> makePolygonWithHole(List<Vector3> polygonPos, List<Vector3> holePos, Vector3[] shortestDots)
        {
            List<Vector3> sortPolygon = sortListbyStartPoint(polygonPos, shortestDots[0]);
            List<Vector3> sortHole = sortListbyStartPoint(holePos, shortestDots[1]);
            List<Vector3> merge = new List<Vector3>();

            foreach (Vector3 v in sortPolygon) merge.Add(v);
            merge.Add(sortPolygon[0]);

            foreach (Vector3 v in sortHole) merge.Add(v);
            merge.Add(sortHole[0]);

            return merge;
        }

        void OnValidate()
        {
            if (numOfTriangle > 0)
            {
                triangluation(numOfTriangle);
            }
        }

        void Start()
        {
            foreach (Transform tr in polygon.transform)
            {
                Vector3 v = tr.transform.position;
                polygonPos.Add(v);
            }

            foreach (Transform tr in hole.transform)
            {
                Vector3 v = tr.transform.position;
                holePos.Add(v);
            }

            GameObject temp;
            Vector3[] shortestDots = getShortestDots(polygonPos, holePos);

            mergePolygon = makePolygonWithHole(polygonPos, holePos, shortestDots);
        }
    }
}