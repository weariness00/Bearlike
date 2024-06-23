using BehaviorTree.Base;
using Fusion;
using Sound;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.VFX;
using DebugManager = Manager.DebugManager;

namespace Monster.Container
{
    public class BoxJester : MonsterBase
    {
        #region Property

        // public SoundBox soundBox;
        private BehaviorTreeRunner _behaviorTreeRunner;
        private bool animationing = false;
        
        [Header("Animator")]
        [SerializeField] private BoxJesterAnimator animator;
        
        [Header("Teleport Properties")]
        public Transform[] tpPlaces;
        public VisualEffect tpEffect;
    
        private int _tpPlaceIndex = 0;
        
        #endregion


        #region Unity Event Function

        void Awake()
        {
            base.Awake();
            
            animator = GetComponentInChildren<BoxJesterAnimator>();
            // tpEffect.SetFloat("Time", animator.tptClip.length);
        }

        #endregion

        #region Member Function

        public override void Spawned()
        {
            base.Spawned();
            tpEffect.SendEvent("StopPlay");

            // Transform rootTrans = transform.root.Find("TPPosition"); // pool에 들어가는 경우
            Transform rootTrans = GameObject.Find("Boss Stage").transform.Find("TPPosition"); // 안들어가는 경우

            if (rootTrans != null)
            {
                tpPlaces = new Transform[rootTrans.childCount];

                for (int i = 0; i < rootTrans.childCount; ++i)
                {
                    DebugManager.Log($"tpPlaces[i] : {tpPlaces[i]}, rootTrans.GetChild(i) : {rootTrans.GetChild(i)}");
                    tpPlaces[i] = rootTrans.GetChild(i);
                }
            }
            else
            {
                DebugManager.LogError($"BoxJester의 TPPosition이 NULL입니다.");
            }
        }

        #endregion

        #region BT Function
        
        public override INode InitBT()
        {
            var Idle = new ActionNode(IdleNode);
        
            var TP = new SequenceNode(
                new ActionNode(TeleportCharge),
                new ActionNode(TeleportAction)
                );

            #region Hide

            var ChangeMask = new SelectorNode(
                true,
                new ActionNode(ChangeSmile),
                new ActionNode(ChangeCry),
                new ActionNode(ChangeAngry)
            );
            
            var HideSelect = new SelectorNode(
                true, 
                new ActionNode(SmokeAttack),
                ChangeMask
            );
            
            var Hide = new SequenceNode(
                new ActionNode(HideOnBox),
                HideSelect,
                new ActionNode(AppearInBox)
            );

            #endregion

            #region Attack

            #region Smile

            

            #endregion

            #region Cry

            

            #endregion
            
            #region Angry
            

            

            #endregion
            
            #endregion

            var AttackPattern = new SelectorNode(
                true, 
                TP,
                Hide
            );
        
            var loop = new SequenceNode(
                Idle,
                AttackPattern
            );

            return loop;
        }

        #region Idle

        private INode.NodeState IdleNode()
        {
            if (false == animationing)
            {
                animator.PlayIdle();
                animationing = true;
            }

            if(false == animator.IdleTimerExpired)
                return INode.NodeState.Running;

            animationing = false;
            return INode.NodeState.Success;
        }

        #endregion

        #region TP

        private INode.NodeState TeleportCharge()
        {
            if (false == animationing)
            {
                tpEffect.SendEvent("OnPlay");
                animator.PlayTeleport();
                animationing = true;
            }

            if (false == animator.TeleportTimerExpired)
                return INode.NodeState.Running;

            animationing = false;
            return INode.NodeState.Success;
        }
        
        private INode.NodeState TeleportAction()
        {
            TPPositionRPC();
            
            return INode.NodeState.Success;
        }

        #endregion

        #region Hide

        private INode.NodeState HideOnBox()
        {
            // 상자에 숨는 애니메이션 실행
            return INode.NodeState.Success;
        }
        
        private INode.NodeState SmokeAttack()
        {
            // VFX실행 및 범위 탐색으로 공격 실행
            return INode.NodeState.Success;
        }

        #region Change Mask

        private INode.NodeState ChangeSmile()
        {
            // 가면 Change ==> 속성 파라미터 변경, 모델 변경(API만들어서)
            return INode.NodeState.Success;
        }
        
        private INode.NodeState ChangeCry()
        {
            // 가면 Change ==> 속성 파라미터 변경, 모델 변경(API만들어서)
            return INode.NodeState.Success;
        }
        
        private INode.NodeState ChangeAngry()
        {
            // 가면 Change ==> 속성 파라미터 변경, 모델 변경(API만들어서)
            return INode.NodeState.Success;
        }

        #endregion
        
        private INode.NodeState AppearInBox()
        {
            // 상자에서 나오는 애니메이션 실행
            return INode.NodeState.Success;
        }
        
        #endregion
        
        #region Attack Pattern

        private INode.NodeState a()
        {
            // 상자에 숨는 애니메이션 실행
            return INode.NodeState.Success;
        }

        #endregion
        
        #endregion

        #region RPC Fuction

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void TPPositionRPC()
        {
            int index = Random.Range(0, 5);

            while (_tpPlaceIndex == index)
                index = Random.Range(0, 5);
            
            DebugManager.Log($"previndex : {_tpPlaceIndex}, curr : {index}");
            
            _tpPlaceIndex = index;
            
            transform.position = tpPlaces[_tpPlaceIndex].position;
        }

        #endregion
    }
}