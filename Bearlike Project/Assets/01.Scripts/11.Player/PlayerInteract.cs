using System;
using Fusion;
using Item;
using Manager;
using Photon;
using UI;
using UnityEngine;
using Util;

namespace Player
{
    public class PlayerInteract : NetworkBehaviourEx, IInteract
    {
        private PlayerController _playerController;

        public float interactLength = 1f; // 상호작용 범위

        private IInteract _currentInteract;
        private bool _isEnterInteract = false;
        private bool _isInteractKeyPress = false;
        
        #region Unity Event Function

        private void Awake()
        {
            _playerController = GetComponent<PlayerController>();
        }

        private void Start()
        {
            InteractInit();
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
            // if(Physics.Raycast(ray, out var hit, interactLength))
            {
                IInteract interact;
                if (hit.GameObject.TryGetComponent(out interact) || hit.GameObject.transform.root.gameObject.TryGetComponent(out interact))
                // if(hit.transform.TryGetComponent(out interact) || hit.transform.root.TryGetComponent(out interact))
                {
                    // 처음 진입 상태인지
                    if (_isEnterInteract == false)
                    {
                        _isInteractKeyPress = false;
                        _isEnterInteract = true;
                        interact.InteractEnterAction?.Invoke(gameObject);
                        _currentInteract = interact;
                    }
                }
                if (interact is { IsInteract: true })
                {
                    // 이미 상호작용중에 다른 상호작용 객체로 바뀌었는지
                    if (interact != _currentInteract)
                    {
                        InteractUI.Instance.SetActiveAll(false);
                        _isInteractKeyPress = false;
                        _currentInteract.InteractExitAction?.Invoke(gameObject);
                        interact.InteractEnterAction?.Invoke(gameObject);
                        
                        _currentInteract = interact;
                    }
                    
                    // 상호작용 키를 눌렀는지
                    if (data.Interact)
                    {
                        _isInteractKeyPress = true;
                        interact.InteractKeyDownAction?.Invoke(gameObject);
                    }
                    // 상호작용 키를 누르고 떗는지
                    else if(_isInteractKeyPress)
                    {
                        _isInteractKeyPress = false;
                        interact.InteractKeyUpAction?.Invoke(gameObject);
                    }
                }
            }
            else
            {
                if (_isEnterInteract == true)
                {
                    _isEnterInteract = false;
                    _currentInteract.InteractExitAction?.Invoke(gameObject);
                    InteractUI.Instance.SetActiveAll(false);
                }
            }
        }

        #region Interact

        public void InteractInit()
        {
            InteractEnterAction += InteractEnter;
            _playerController.status.InjuryAction += () => IsInteract = true;
            _playerController.status.RecoveryFromInjuryAction += () => IsInteract = false;
            InteractKeyDownAction += InjuryInteractKeyDown;
            InteractKeyDownAction += DieInteract;
            InteractKeyUpAction += InjuryInteractKeyUp;
        }

        public bool IsInteract { get; set; }
        public Action<GameObject> InteractEnterAction { get; set; }
        public Action<GameObject> InteractExitAction { get; set; }
        public Action<GameObject> InteractKeyDownAction { get; set; }
        public Action<GameObject> InteractKeyUpAction { get; set; }

        void InteractEnter(GameObject targetObject)
        {
            if (_playerController.status.isInjury || _playerController.status.isRevive)
            {
                InteractUI.SetKeyActive(true);
                InteractUI.KeyCodeText.text = "F";
            }
        }
        
        public void InjuryInteractKeyDown(GameObject targetObject)
        {
            if (_playerController.status.isInjury)
            {
                var remotePlayerStatus = targetObject.GetComponent<PlayerStatus>();
                
                InteractUI.SetGageActive(true);
                InteractUI.GageSlider.value = remotePlayerStatus.recoveryFromInjuryTime.Current / remotePlayerStatus.recoveryFromInjuryTime.Max;
                
                remotePlayerStatus.SetHelpOtherPlayerRPC(true); // 현재 상호작용 중인 플레이어가 다른 플레이어에게 도움을 주고 있음을 알린다.
                remotePlayerStatus.SetRecoveryInjuryTimeRPC(remotePlayerStatus.recoveryFromInjuryTime.Current + Time.deltaTime);

                if (remotePlayerStatus.recoveryFromInjuryTime.isMax)
                {
                    remotePlayerStatus.SetHelpOtherPlayerRPC(false); 
                    remotePlayerStatus.SetRecoveryInjuryTimeRPC(0); 
                    _playerController.status.RecoveryFromInjuryActionRPC();
                }
            }
        }

        public void InjuryInteractKeyUp(GameObject targetObject)
        {
            InteractUI.SetGageActive(false);
        }

        public void DieInteract(GameObject targetObject)
        {
            if (_playerController.status.isRevive)
            {
                var remotePlayerController = targetObject.GetComponent<PlayerController>();
                var battery = ItemObjectList.GetFromName("Battery");
                if (remotePlayerController.itemInventory.HasItem(battery.Id))
                {
                    remotePlayerController.itemInventory.UseItemRPC(new NetworkItemInfo(){Id = battery.Id, amount = 1});
                    _playerController.status.RecoveryFromReviveActionRPC(); // 대상을 부활
                }

                DebugManager.ToDo("배터리가 있는 플레이어만 살릴 수 있어야 된다. 배터리가 있는지에 대한 여부와 살릴떄 배터리를 사용했다는 것을 알려줘야한다.");
            }
        }

        #endregion
    }
}