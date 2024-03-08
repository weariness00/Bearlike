using System;
using BehaviorTree.Base;
using Status;
using UnityEngine;
using Util;
using Random = UnityEngine.Random;

namespace Monster.Container
{
    [RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
    public class MonsterDice : MonsterBase
    {
        private BoxCollider _collider;
        private Rigidbody _rigidbody;
        private BehaviorTreeRunner _behaviorTreeRunner;

        private bool _isCollide = false; // 현재 충돌 중인지
        public Vector3 moveDirection; // 움직이려는 방향
        public StatusValue<float> moveDelay = new StatusValue<float>(); // 몇초에 한번씩 움직일지 ( 자연스러운 움직임 구현을 위해 사용 )

        public override void Start()
        {
            base.Start();
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<BoxCollider>();

            moveDirection = Random.onUnitSphere.normalized;
        }

        private void OnCollisionEnter(Collision other)
        {
            _isCollide = true;
        }

        private void OnCollisionExit(Collision other)
        {
            _isCollide = false;
        }

        private INode InitBT()
        {
            var sqeunce = new SequenceNode(
                new ActionNode()
                );
        
            return sqeunce;
        }

        #region Function

        public Vector3 SetRotateDir(Vector3 destinationPosition)
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

        #endregion

        #region BT Function

        private void Move()
        {
            moveDelay.Current = Runner.DeltaTime;
            if (_isCollide && moveDelay.isMax)
            {
                moveDelay.Current = moveDelay.Min;
                _rigidbody.AddTorque(_rigidbody.mass * 110 * moveDirection);
            }
        }

        #endregion
    }
}

