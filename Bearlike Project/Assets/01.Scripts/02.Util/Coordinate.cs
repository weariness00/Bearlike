using System;
using UnityEngine;

namespace Util
{
    [Serializable]
    public class Coordinate
    {
        public Vector3 right = Vector3.zero;
        public Vector3 up = Vector3.zero;
        public Vector3 forward = Vector3.zero;

        public void CalFromUp()
        {
            Vector3 arbitraryForward = Vector3.forward;
            if (Vector3.Dot(up, arbitraryForward) > 0.999f)
            {
                arbitraryForward = Vector3.right;
            }
            right = Vector3.Cross(up, arbitraryForward).normalized;
            forward = Vector3.Cross(right, up).normalized;
        }

        public void CalFromForward()
        {
            right = Vector3.Cross(forward, Vector3.up).normalized;
            up = Vector3.Cross(right, forward).normalized;
        }
    }
}