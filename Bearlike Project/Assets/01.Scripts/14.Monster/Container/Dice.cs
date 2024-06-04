using System;
using System.Collections.Generic;
using System.Linq;
using Status;
using BehaviorTree.Base;
using Data;
using Fusion;
using GamePlay.DeadBodyObstacle;
using Manager;
using Photon.MeshDestruct;
using Player;
using UI.Status;
using UnityEngine;
using UnityEngine.AI;
using Util;
using Random = UnityEngine.Random;

namespace Monster.Container
{
    public class Dice : MonsterBase
    {
        public DiceAnimator animatorInfo;
        public NetworkPrefabRef diePrefab;
        
        private BehaviorTreeRunner _behaviorTreeRunner;
        private bool _isCollide = true; // 현재 충돌 중인지
        private float _moveDelay; // 몇초에 한번씩 움직일지 1번의 움직임이 1m움직임이라 가정( 자연스러운 움직임 구현을 위해 사용 )
        [Networked] private TickTimer MoveDelayTimer { get; set; }
        [Networked] private TickTimer AttackTimer { get; set; }
        private bool _isInitAnimation = false;

        #region Unity Evenet Function

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
            base.Spawned();
            MoveDelayTimer = TickTimer.CreateFromSeconds(Runner, 0);
            AttackTimer = TickTimer.CreateFromSeconds(Runner, 0);
            
            _behaviorTreeRunner = new BehaviorTreeRunner(InitBT());
            _moveDelay = 1f / status.GetMoveSpeed();

            DieAction += DeadSlice;
        }

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();
             _behaviorTreeRunner.Operator();
        }
        
        #endregion

        #region Member Function

        public void SpearAttack()
        {
            LayerMask mask = targetMask | 1 << LayerMask.NameToLayer("Default");
            Vector3[] attackDirs = new[] { transform.forward, -transform.forward, transform.right, -transform.right, transform.up, -transform.up };
            foreach (var attackDir in attackDirs)
            {
                DebugManager.DrawRay(transform.position, attackDir* status.attackRange, Color.red, 3f);
                List<LagCompensatedHit> hits = new List<LagCompensatedHit>();
                if (Runner.LagCompensation.RaycastAll(transform.position, attackDir, status.attackRange, Object.InputAuthority, hits, mask) != 0)
                {
                    StatusBase targetStatus;
                    foreach (var hit in hits)
                    {
                        if (hit.GameObject.TryGetComponent(out targetStatus) || hit.GameObject.transform.root.TryGetComponent(out targetStatus))
                        {
                            targetStatus.ApplyDamageRPC(status.CalDamage(out var isCritical), isCritical ? DamageTextType.Critical : DamageTextType.Normal, Object.Id,CrowdControl.Normality);
                        }
                    }
                }
            }
        }
        
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

        #endregion

        #region BT Function

        private INode InitBT()
        {
            var findTarget = new ActionNode(FindTarget);
            var move = new ActionNode(Move);
            var attack = new ActionNode(Attack);
            var jumpAttack = new ActionNode(JumpAttack);
            var selectAttack = new SelectorNode(
                true, 
                new Detector(() => CheckNavMeshDis(status.attackRange.Current), attack)
                );
            DebugManager.ToDo("Jump Attack 추가하기");
            
            var sqeunce = new SequenceNode(
                findTarget,
                new SelectorNode(
                    false,
                    selectAttack,
                    move
                ));
            return sqeunce;
        }

        private INode.NodeState Move()
        {
            if (CheckNavMeshDis(status.attackRange.Current - 0.1f))
            {
                return INode.NodeState.Success;
            }
            
            if (_isCollide && MoveDelayTimer.Expired(Runner))
            {
                Vector3 dir = Vector3.zero;
                MoveDelayTimer = TickTimer.CreateFromSeconds(Runner, _moveDelay);

                if (targetPlayer == null)
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
                    if (NavMesh.CalculatePath(transform.position, targetPlayer.transform.position, NavMesh.AllAreas, path))
                    {
                        if (path.corners.Length > 1)
                        {
                            dir = SetRotateDir(path.corners[1]);
                        }
                    }
                    else
                    {
                        dir = SetRotateDir(targetPlayer.transform.position);
                    }
                }

                dir = 300f * rigidbody.mass * status.GetMoveSpeed() * dir;
                rigidbody.AddTorque(dir);

                return INode.NodeState.Success; 
            }
            return INode.NodeState.Running;
        }

        private INode.NodeState Attack()
        {
            if (_isInitAnimation)
            {
                AttackTimer = TickTimer.CreateFromSeconds(Runner, animatorInfo.AttackTime);
                animatorInfo.PlayAttack();
                _isInitAnimation = false;
            }

            if (AttackTimer.Expired(Runner) == false)
            {
                return INode.NodeState.Running;
            }

            _isInitAnimation = true;
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
                    playerStatus.ApplyDamageRPC(status.CalDamage(out var isCritical), isCritical ? DamageTextType.Critical : DamageTextType.Normal, Object.Id, CrowdControl.Normality);
                }
            }
            return INode.NodeState.Failure;
        }

        #endregion
    }
}

