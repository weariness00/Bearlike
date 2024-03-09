﻿using System;
using BehaviorTree.Base;
using Script.Data;
using Status;
using UnityEngine;
using UnityEngine.AI;
using Util;
using Random = UnityEngine.Random;

namespace Monster.Container
{
    [RequireComponent(typeof(BoxCollider))]
    public class MonsterDice : MonsterBase
    {
        private BehaviorTreeRunner _behaviorTreeRunner;
        private bool _isCollide = false; // 현재 충돌 중인지
        public StatusValue<float> moveDelay = new StatusValue<float>(); // 몇초에 한번씩 움직일지 ( 자연스러운 움직임 구현을 위해 사용 )

        public override void Start()
        {
            base.Start();
            _behaviorTreeRunner = new BehaviorTreeRunner(InitBT());
        }

        private void OnCollisionStay(Collision other)
        {
            _isCollide = true;
        }

        private void OnCollisionExit(Collision other)
        {
            _isCollide = false;
        }

        public override void FixedUpdateNetwork()
        {
            _behaviorTreeRunner.Operator();
        }

        private INode InitBT()
        {
            var sqeunce = new SequenceNode(
                new ActionNode(FindTarget),
                new ActionNode(Move)
                );
        
            return sqeunce;
        }

        #region Function

        public Vector3 SetRotateDir(Vector3 destinationPosition)
        {
            var destinationDir = (transform.position - destinationPosition);
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
            coordinate.right.y = 0;
            coordinate.right.Normalize();
            
            return coordinate.right.normalized;
        }

        #endregion

        #region BT Function

        private INode.NodeState FindTarget()
        {
            if (targetTransform == null)
            {
                foreach (var (playerRef, data) in UserData.Instance.UserDictionary)
                {
                    var playerObject = Runner.FindObject(data.NetworkId);
                    var path = new NavMeshPath();
                    if (playerObject != null &&
                        NavMesh.CalculatePath(transform.position, playerObject.transform.position, NavMesh.AllAreas, path))
                    {
                        // 해당 Player까지의 거리 계산
                        var dis = 0f;
                        for (int i = 0; i < path.corners.Length - 1; i++)
                        {
                            dis += Vector3.Distance(path.corners[i], path.corners[i + 1]);
                        }
                        
                        // 범위 내라면 해당 플레이어를 타겟으로 설정
                        if (dis < 15f)
                        {
                            targetTransform = playerObject.transform;
                            break;
                        }
                    }
                }
            }
            else
            {
                // 타겟까지와의 거리를 판단하여 멀어지면 타겟 해제
                var path = new NavMeshPath();
                if (NavMesh.CalculatePath(transform.position, targetTransform.position, NavMesh.AllAreas, path))
                {
                    var dis = 0f;
                    for (int i = 0; i < path.corners.Length - 1; i++)
                    {
                        dis += Vector3.Distance(path.corners[i], path.corners[i + 1]);
                    }
                        
                    // 범위 내라면 해당 플레이어를 타겟으로 설정
                    if (dis > 20f)
                    {
                        targetTransform = null;
                    }
                }
            }

            return INode.NodeState.Success; 
        }

        private INode.NodeState Move()
        {
            moveDelay.Current += Runner.DeltaTime;
            if (_isCollide && moveDelay.isMax)
            {
                Vector3 dir = Vector3.zero;
                if (targetTransform == null)
                {
                    // 타겟이 없으면 자유로운 방향으로 이동하게 하기
                    dir = SetRotateDir(Random.insideUnitCircle);
                }
                else 
                {
                    var path = new NavMeshPath();
                    NavMeshQueryFilter filter = new NavMeshQueryFilter();
                    filter.areaMask = 1 << NavMesh.GetAreaFromName("Walkable");
                    if (NavMesh.CalculatePath(transform.position, targetTransform.position, NavMesh.AllAreas, path))
                    {
                        dir = SetRotateDir(path.corners[0]);
                    }
                }

                dir = rigidbody.mass * status.moveSpeed * dir;
                rigidbody.AddTorque(dir);

                moveDelay.Current = moveDelay.Min;
                return INode.NodeState.Success; 
            }
            return INode.NodeState.Running;
        }

        #endregion
    }
}
