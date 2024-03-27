using System;
using Fusion;
using Manager;
using Photon;
using Status;
using UnityEngine;
using Util;

namespace Player
{
    public class PlayerInteract : NetworkBehaviourEx, IInteract
    {
        private PlayerController _playerController;

        public float interactLength = 1f; // 상호작용 범위

        #region Unity Event Function

        private void Start()
        {
            _playerController = GetComponent<PlayerController>();
        }

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out PlayerInputData data))
            {
                CheckInteract(data);
            }
        }

        #endregion
        
        void CheckInteract(PlayerInputData data)
        {
            if (HasInputAuthority == false)
            {
                return;
            }
            
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            var hitOptions = HitOptions.IncludePhysX | HitOptions.IgnoreInputAuthority;
            DebugManager.DrawRay(ray.origin, ray.direction * interactLength, Color.red, 1.0f);
            if (Runner.LagCompensation.Raycast(ray.origin, ray.direction, interactLength, Object.InputAuthority, out var hit, Int32.MaxValue, hitOptions))
            {
                var interact = hit.GameObject.GetComponent<IInteract>();
                if (interact is { IsInteract: true })
                {
                    if (data.Interact)
                    {
                        interact.InteractEnterAction?.Invoke(gameObject);
                    }
                    else
                    {
                        interact.InteractExitAction?.Invoke(gameObject);
                    }
                    DebugManager.ToDo("상호작용이 가능할 경우 상호작용 키 UI 띄어주기");
                }
            }
        }

        #region Interact

        public void InteractInit()
        {
            _playerController.status.injuryAction += () => IsInteract = true;
            InteractEnterAction += InjuryInteractEnter;
            InteractEnterAction += DieInteract;
        }

        public bool IsInteract { get; set; }
        public Action<GameObject> InteractEnterAction { get; set; }
        public Action<GameObject> InteractExitAction { get; set; }

        public void InjuryInteractEnter(GameObject targetObject)
        {
            if (_playerController.status.isInjury)
            {
                var remotePlayerStatus = targetObject.GetComponent<PlayerStatus>();
                remotePlayerStatus.SetHelpOtherPlayerRPC(true); // 현재 상호작용 중인 플레이어가 다른 플레이어에게 도움을 주고 있음을 알린다.
                if (remotePlayerStatus.isHelpOtherPlayer)
                {
                    remotePlayerStatus.SetRecoveryInjuryTimeRPC(remotePlayerStatus.recoveryFromInjuryTime.Current + Runner.DeltaTime);
                }

                if (remotePlayerStatus.recoveryFromInjuryTime.isMax)
                {
                    // 부상 회복
                    _playerController.status.SetHpRPC(StatusValueType.Current, _playerController.status.hp.Max / 3);
                    
                    // remote Player의 부상 관련 스테이터스 초기화
                    remotePlayerStatus.SetIsInjuryRPC(false);
                    remotePlayerStatus.SetRecoveryInjuryTimeRPC(0);
                    IsInteract = false;
                }
            }
        }

        public void DieInteract(GameObject targetObject)
        {
            if (_playerController.status.isRevive)
            {
                var remotePlayerStatus = targetObject.GetComponent<PlayerStatus>();
                
                DebugManager.ToDo("배터리가 있는 플레이어만 살릴 수 있어야 된다. 배터리가 있는지에 대한 여부와 살릴떄 배터리를 사용했다는 것을 알려줘야한다.");
            }
        }
        
        #endregion

    }
}