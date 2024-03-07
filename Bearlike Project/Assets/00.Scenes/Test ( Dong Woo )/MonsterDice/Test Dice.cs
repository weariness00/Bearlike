using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _00.Scenes.Test___Dong_Woo__.MonsterDice
{
    [RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
    public class TestDice : MonoBehaviour
    {
        public int speed = 10;
        private Rigidbody _rigidbody;

        public float moveDelay = 1f;
        public bool _isCollide;
        public Vector3 collideNormal;
        public Vector3 moveDir;
        public float meshSize;

        public Transform TargetTransform;
        
        // Start is called before the first frame update
        void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
            var mesh = GetComponent<MeshFilter>().sharedMesh;
            meshSize = (mesh.bounds.max - mesh.bounds.min).magnitude;
        }

        // Update is called once per frame
        void Update()
        {
            moveDelay += Time.deltaTime;
            if (_isCollide && moveDelay > 1f)
            {
                // moveDir을 forward로 잡고 up, right를 구한다.
                // Coordinate moveCoordinate = new Coordinate();
                // moveCoordinate.forward = moveDir;
                // moveCoordinate.CalFromForward();
                //
                // Debug.DrawRay(transform.position, moveDir * meshSize * transform.localScale.magnitude, Color.red, 5f);
                // if(Physics.BoxCast(transform.position, transform.localScale / 2f, moveDir, quaternion.identity))
                // {
                //     Vector3 arbitraryForward = Vector3.forward;
                //     if (Vector3.Dot(collideNormal, arbitraryForward) > 0.999f)
                //     {
                //         arbitraryForward = Vector3.right;
                //     }
                //     Vector3 rightVector = Vector3.Cross(collideNormal, arbitraryForward).normalized;
                //     Vector3 forwardVector = Vector3.Cross(rightVector, collideNormal).normalized;
                //
                //     moveDir = GetRandomPointOnCircle(1f, collideNormal);
                // }
                // else
                {
                    moveDir = SetMoveDir(TargetTransform.position);
                    _rigidbody.AddTorque(moveDir * 110 * _rigidbody.mass);
                    moveDelay = 0f;
                }
            }
        }
        
        private void OnCollisionEnter(Collision other)
        {
            _isCollide = true;
            collideNormal = other.contacts[0].normal;
        }

        private void OnCollisionExit(Collision other)
        {
            _isCollide = false;
        }
        
        Vector3 GetRandomPointOnCircle(float radius, Vector3 axis)
        {
            Vector3 randomPoint = Vector3.zero;

            // 중심축에 직교하는 벡터를 찾습니다.
            Vector3 orthogonalVector = Vector3.Cross(axis, Random.insideUnitSphere).normalized;
            // 원의 평면에 있는 두 번째 직교 벡터를 계산합니다.
            Vector3 binormal = Vector3.Cross(axis, orthogonalVector).normalized;

            // 랜덤한 각도를 라디안으로 변환합니다.
            float angle = Random.Range(0, 2 * Mathf.PI);

            // 원 위의 점을 계산합니다.
            randomPoint = Mathf.Cos(angle) * orthogonalVector * radius + Mathf.Sin(angle) * binormal * radius;

            return randomPoint;
        }

        public Vector3 SetMoveDir(Vector3 destinationPosition)
        {
            var destinationDir = (transform.position - destinationPosition);
            if (destinationDir.magnitude < 2f)
                return Vector3.zero;
            
            var normalize = destinationDir.normalized;
            
            Tuple<float, Vector3>[] angles = new []
            {
                new Tuple<float, Vector3>(Vector3.Angle(normalize, transform.forward), transform.forward),
                new Tuple<float, Vector3>(Vector3.Angle(normalize, -transform.forward), -transform.forward),
                new Tuple<float, Vector3>(Vector3.Angle(normalize, transform.right), transform.right),
                new Tuple<float, Vector3>(Vector3.Angle(normalize, -transform.right), -transform.right),
                new Tuple<float, Vector3>(Vector3.Angle(normalize, transform.up), transform.up),
                new Tuple<float, Vector3>(Vector3.Angle(normalize, -transform.up), -transform.up),
            };

            // 회전을 하여 움직이는 것임으로 2번째로 각도가 작은 값을 찾아준다.
            Tuple<float, Vector3> min = new Tuple<float, Vector3>(float.MaxValue, Vector3.zero);
            foreach (var angle in angles)
            {
                if (angle.Item1 < min.Item1)
                {
                    min = angle;
                }
            }

            Coordinate coordinate = new Coordinate();
            coordinate.forward = min.Item2;
            coordinate.CalFromForward();
            
            return coordinate.right;
        }

        public void Move()
        {
            
        }

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
}
