﻿using System;
using System.Linq;
using Status;
using BehaviorTree.Base;
using Data;
using Fusion;
using GamePlay.DeadBodyObstacle;
using Manager;
using Photon.MeshDestruct;
using UnityEngine;
using UnityEngine.AI;
using Util;
using Random = UnityEngine.Random;

namespace Monster.Container
{
    public class Dice : MonsterBase
    {
        public NetworkPrefabRef diePrefab;
        
        private BehaviorTreeRunner _behaviorTreeRunner;
        private bool _isCollide = true; // 현재 충돌 중인지
        private float _moveDelay; // 몇초에 한번씩 움직일지 1번의 움직임이 1m움직임이라 가정( 자연스러운 움직임 구현을 위해 사용 )
        [Networked] private TickTimer MoveDelayTimer { get; set; }
        
        public override void Start()
        {
            base.Start();
            _behaviorTreeRunner = new BehaviorTreeRunner(InitBT());
            _moveDelay = 1f / status.moveSpeed;

            DieAction += DeadSlice;
        }

        private void OnCollisionStay(Collision other)
        {
            _isCollide = true;
        }

        private void OnCollisionExit(Collision other)
        {
            _isCollide = false;
        }

        public override void Spawned()
        {
            MoveDelayTimer = TickTimer.CreateFromSeconds(Runner, 0);
        }

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();
             _behaviorTreeRunner.Operator();
        }

        private INode InitBT()
        {
            var move = new ActionNode(Move);
            var attack = new ActionNode(Attack);
            var jumpAttack = new ActionNode(JumpAttack);
            var selectAttack = new SelectorNode(true, attack, jumpAttack);
            
            var sqeunce = new SequenceNode(
                new ActionNode(FindTarget),
                new SelectorNode(
                    false,
                    new Detector(() => CheckTargetDis(1f), selectAttack),
                    move
                ));
            return sqeunce;
        }

        #region Member Function

        // 사망시 슬라이스 되도록
        async void DeadSlice()
        {
            if(!HasStateAuthority) return;
            
            var dieObj = await Runner.SpawnAsync(diePrefab, transform.position, transform.rotation);
            NetworkMeshSliceSystem.Instance.SliceRPC(dieObj.Id, Random.onUnitSphere, transform.position, 1f, false, SerializeComponentString(typeof(DeadBodyObstacleObject)));
            
            Destroy(gameObject);
        }
        
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
        
        /// <summary>
        /// 타겟과의 거리를 판단하여 인자로 넣은 값과 비교해 거리가 인자보다 낮으면 true
        /// </summary>
        /// <param name="checkDis">이 거리보다 낮으면 True, 높으면 False</param>
        /// <returns></returns>
        private bool CheckTargetDis(float checkDis)
        {
            if (targetTransform == null)
            {
                return false;
            }
            var dis = NavMeshDistanceFromTarget(targetTransform.position);
            return dis < checkDis;
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
            if (_isCollide && MoveDelayTimer.Expired(Runner))
            {
                Vector3 dir = Vector3.zero;
                if (targetTransform == null)
                {
                    // 타겟이 없으면 자유로운 방향으로 이동하게 하기
                    var randomCircle = Random.insideUnitCircle;
                    dir = SetRotateDir(transform.position + new Vector3(randomCircle.x, 0, randomCircle.y));
                }
                else 
                {
                    var path = new NavMeshPath();
                    NavMeshQueryFilter filter = new NavMeshQueryFilter();
                    filter.areaMask = 1 << NavMesh.GetAreaFromName("Walkable");
                    if (NavMesh.CalculatePath(transform.position, targetTransform.position, NavMesh.AllAreas, path))
                    {
                        if (path.corners.Length > 1)
                        {
                            dir = SetRotateDir(path.corners[1]);
                        }
                    }
                    else
                    {
                        dir = SetRotateDir(targetTransform.position);
                    }
                }

                dir = 300f * rigidbody.mass * status.moveSpeed * dir;
                rigidbody.AddTorque(dir);

                MoveDelayTimer = TickTimer.CreateFromSeconds(Runner, _moveDelay);
                return INode.NodeState.Success; 
            }
            return INode.NodeState.Running;
        }

        private INode.NodeState Attack()
        {
            var layerMaks = LayerMask.GetMask("Player");
            var hitOptions = HitOptions.IncludePhysX | HitOptions.IgnoreInputAuthority;
            Vector3[] attackDirs = new[] { transform.forward, -transform.forward, transform.right, -transform.right, transform.up, -transform.up };
            foreach (var attackDir in attackDirs)
            {
                DebugManager.DrawRay(transform.position, attackDir* status.attackRange, Color.red, 3f);
                if (Runner.LagCompensation.Raycast(transform.position, attackDir, status.attackRange, Object.InputAuthority, out var hit, layerMaks, hitOptions))
                {
                    if (hit.Hitbox == null) return INode.NodeState.Failure;
                    var hitStatus = hit.GameObject.GetComponent<StatusBase>();
                    hitStatus.ApplyDamageRPC(status.CalDamage(), Object.Id,CrowdControl.Normality);
                }
            }

            return INode.NodeState.Success;
        }

        private INode.NodeState JumpAttack()
        {
            Vector3 origin = transform.position;
            float radius = 3f;
            var hits = Physics.SphereCastAll(origin, radius, Vector3.zero, 0, targetMask);
            DebugManager.DrawSphereRay(origin, Vector3.zero, radius, Color.red, 3f);
            foreach (var hit in hits)
            {
                StatusBase playerStatus;
                if (hit.transform.TryGetComponent(out playerStatus) || hit.transform.parent.TryGetComponent(out playerStatus))
                {
                    playerStatus.ApplyDamageRPC(status.CalDamage(), Object.Id, CrowdControl.Normality);
                }
            }
            return INode.NodeState.Failure;
        }

        #endregion
    }
}

