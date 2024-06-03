using Photon;
using UnityEngine;

namespace Test
{
    public class TestLayController : NetworkBehaviourEx
    {
        public float distance = 1f;
        public LayerMask includeMask;
        public Transform originTransform;
        public Transform directionTransform;

        private void Awake()
        {
            if (!originTransform) originTransform = transform;
            if (!directionTransform)
            {
                GameObject obj = new GameObject("Direction");
                obj.transform.position = transform.forward;
                obj.transform.SetParent(transform);
            }
        }

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();

            if (Input.GetKeyDown(KeyCode.F))
            {
                Vector3 dir = directionTransform.position - originTransform.position;
                Debug.DrawRay(originTransform.position, dir.normalized * distance, Color.blue, 2f);
                if (Physics.Raycast(originTransform.position, dir.normalized, out var hit, distance, includeMask))
                {
                    Debug.Log($"Physics 충돌 [Object : {hit.transform.name}]", hit.transform.gameObject);
                }
            }
            else if (Input.GetKeyDown(KeyCode.G))
            {
                Vector3 dir = directionTransform.position - originTransform.position;
                Debug.DrawRay(originTransform.position, dir.normalized * distance, Color.blue, 2f);
                if (Runner.LagCompensation.Raycast(originTransform.position, dir.normalized, distance, Runner.LocalPlayer, out var hit, includeMask))
                {
                    Debug.Log($"Fusion 충돌 [Object : {hit.GameObject.name}]", hit.GameObject);
                }
            }
        }
    }
}