using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Status;
using Fusion;
using Manager;
using Monster;
using Photon;
using Player;
using UI.Status;
using UnityEngine;
using UnityEngine.VFX;
using Util;

namespace Skill.Container
{
    public class CleanShoot : SkillBase
    {
        public Canvas cleanShootCanvas;
        public Canvas aimCanvas;
        public GameObject areaObject;
        public GameObject aimObject;

        // public VisualEffect trajectoryVFX; // 총알 궤적 이펙트
        public NetworkPrefabRef trajectoryVFX;
        private float _trajectoryVFXDestroyTime;
        public Vector2 range;

        private RectTransform _areaRect;
        private Animation areaAnimation;
        private float _aniAreaOpenTime;
        private WaitForSeconds _areaOpenWaiter; // UI 키는 애니메이션 시간
        private float _aniAreaCloseTime;
        private WaitForSeconds _areaCloseWaiter; // UI 키는 애니메이션 시간

        private Animation _aimAnimation;
        private WaitForSeconds _aimTargetingAniTime;

        private LayerMask _layerMask;

        private TickTimer _cancelTimer; // 스킬을 취소 할 수 있게 하는데 바로 하지 않고 스킬 발동후 일정 시간 이후에 하게 하기 위해 사용
        private WaitForSeconds _findTime; // 몇초를 주기로 영역내에 몬스터 포착 업데이트를 하게 할지

        private List<MonsterBase> _monsterList = new List<MonsterBase>(); // 타격할 몬스터를 담는 컨테이너
        private Dictionary<GameObject, GameObject> _aimDictionary = new Dictionary<GameObject, GameObject>(); // 타격할 몬스터를 UI로 표시할때 UI들을 담는 컨테이너 // L : Target , R : Aim
        private bool _isAttack;

        public override void Awake()
        {
            base.Awake();

            {
                areaAnimation = areaObject.GetComponent<Animation>();
                _areaRect = areaObject.GetComponent<RectTransform>();
                
                var openClip = areaAnimation.GetClip("Open Clean Shoot Area");
                _aniAreaOpenTime = openClip.length;
                _areaOpenWaiter = new WaitForSeconds(openClip.length);
                
                var closeClip = areaAnimation.GetClip("Close Clean Shoot Area");
                _aniAreaCloseTime = closeClip.length;
                _areaCloseWaiter = new WaitForSeconds(closeClip.length);
            }

            {
                _aimAnimation = aimObject.GetComponent<Animation>();
                var targetClip = _aimAnimation.GetClip("Clean Shoot Aim Targeting");
                _aimTargetingAniTime = new WaitForSeconds(targetClip.length);
            }

            {
                // _trajectoryVFXDestroyTime = trajectoryVFX.GetFloat("Duration");
                _trajectoryVFXDestroyTime = 3f;
            }

            _layerMask = 1 << LayerMask.GetMask("Default");

            _findTime = new WaitForSeconds(0.2f);
        }

        public override void Spawned()
        {
            base.Spawned();
            _cancelTimer = TickTimer.CreateFromSeconds(Runner, 0f);
        }

        #region Skill Base Function

        public override void Earn(GameObject earnTargetObject)
        {
            base.Earn(earnTargetObject);

            if (earnTargetObject.TryGetComponent(out PlayerController pc))
            {
                status.AddAdditionalStatus(pc.status);
            }
        }

        public override void MainLoop()
        {
        }

        public override void Run()
        {
            if(_cancelTimer.Expired(Runner) == false) return;
            
            if (IsUse && isInvoke == false)
            {
                _cancelTimer = TickTimer.CreateFromSeconds(Runner, _aniAreaOpenTime);

                isInvoke = true;
                _areaRect.sizeDelta = range;

                if (HasInputAuthority)
                {
                    StopAllCoroutines();
                    
                    cleanShootCanvas.gameObject.SetActive(true);
                    aimCanvas.gameObject.SetActive(true);
                    ownerPlayer.cameraController.SetLensDistortion(-0.2f, 1f, 1f, null, 1.05f, _aniAreaOpenTime);
                    StartCoroutine(AreaOpenSetting(ownerPlayer.gameObject));
                }

                StartCoroutine(AttackMonsterFromArea());
            }
            else if (isInvoke)
            {
                _cancelTimer = TickTimer.CreateFromSeconds(Runner, _aniAreaCloseTime);
                
                isInvoke = false;
                if (HasInputAuthority)
                {
                    ownerPlayer.cameraController.SetLensDistortion(0f, 1f,1f, null, 1f, _aniAreaCloseTime);
                    areaAnimation.Play("Close Clean Shoot Area");
                    StopAllCoroutines();
                    foreach (var (monster, aim) in _aimDictionary)
                        Destroy(aim);
                    aimCanvas.gameObject.SetActive(false);

                    StartCoroutine(AreaCloseSetting());
                }
                _aimDictionary.Clear();
            }
        }

        public override void LevelUp()
        {
            base.LevelUp();

            ExplainUpdate();
        }

        public override void ExplainUpdate()
        {
            base.ExplainUpdate();
            if (explain.Contains("(현재 공격력)"))
                explain = explain.Replace("(현재 공격력)", $"({status.CalDamage(out bool isCritical)})");
            if (explain.Contains("(Level)"))
                explain = explain.Replace("(Level)", $"{level.Current}");
            
            explain = StringExtension.Replace(explain);
        }
        
        #endregion

        IEnumerator AttackMonsterFromArea()
        {
            yield return _areaOpenWaiter;
            while (true)
            {
                yield return null;
                if (KeyManager.InputAction(KeyToAction.Attack))
                {
                    // 스킬을 사용했으면 초기화 해야됨
                    isInvoke = false;
                    SetSkillCoolTimerRPC(coolTime);
                    cleanShootCanvas.gameObject.SetActive(false);
                    aimCanvas.gameObject.SetActive(false);

                    foreach (var (monster, aim) in _aimDictionary)
                        Destroy(aim);
                    _aimDictionary.Clear();

                    if (HasInputAuthority)
                    {
                        ownerPlayer.cameraController.SetLensDistortion();

                        foreach (var monster in _monsterList)
                        {
                            var targetStatus = monster.GetComponent<MonsterStatus>();
                            targetStatus.ApplyDamageRPC(status.CalDamage(out var isCritical), isCritical ? DamageTextType.Critical : DamageTextType.Normal, ownerPlayer.Object.Id, CrowdControl.Normality);

                            // 총알 궤적 VFX 생성
                            var monsterNetworkId = monster.GetComponent<NetworkObject>().Id;
                            var viewPosition = Camera.main.ViewportToWorldPoint(new Vector3(.5f, .5f, 1f));
                            SpawnTrajectoryRPC(monsterNetworkId, viewPosition);
                        }
                    }

                    break;
                }
            }
        }

        IEnumerator AreaOpenSetting(GameObject runObject)
        {
            // viewport상의 영역 크기 잡아주기
            var rangeHalf = range / 2f;
            var screen = new Vector2(Screen.width, Screen.height);
            var screenHalf = screen / 2f;
            var areaMin = (screenHalf - rangeHalf) / screen;
            var areaMax = (screenHalf + rangeHalf) / screen;

            // 영역 여는 애니메이션 길이만큼 대기
            areaAnimation.Play("Open Clean Shoot Area");
            yield return _areaOpenWaiter;

            DebugManager.ToDo("영역에 잡힌 Monster들의 위치를 UI로 띄어주기");

            while (true)
            {
                yield return _findTime;
                if (isInvoke == false)
                {
                    break;
                }

                var monsters = FindObjectsOfType<MonsterBase>();

                _monsterList.Clear();
                var runPosition = Camera.main.transform.position;
                foreach (var monster in monsters)
                {
                    Vector3 monsterViewportPosition = Camera.main.WorldToViewportPoint(monster.transform.position);

                    // monsterViewportPosition의 값이 areaMin, areaMax 값 사이에 있으면 객체가 UI 영역 내에 있다고 판단할 수 있습니다.
                    if (monsterViewportPosition.x >= areaMin.x && monsterViewportPosition.x <= areaMax.x &&
                        monsterViewportPosition.y >= areaMin.y && monsterViewportPosition.y <= areaMax.y &&
                        monsterViewportPosition.z > 0)
                    {
                        var monsterPosition = monster.pivot.position;
                        var dir = (monsterPosition - runPosition);

                        // ray를 발사해 앞에 장애물이 있는지 확인
                        DebugManager.DrawRay(runPosition, dir, Color.blue, 3f);
                        // if(Physics.Raycast(runPosition, dir.normalized, out var hit, dir.magnitude) == false)
                        if (Runner.LagCompensation.Raycast(runPosition, dir.normalized, dir.magnitude, Runner.LocalPlayer, out var hit) == false)
                        {
                            _monsterList.Add(monster);
                            if (_aimDictionary.ContainsKey(monster.gameObject) == false)
                            {
                                StartCoroutine(AimSetting(runObject, monster.gameObject));
                            }
                        }
                    }
                }
            }
        }

        IEnumerator AreaCloseSetting()
        {
            yield return _areaCloseWaiter;
            cleanShootCanvas.gameObject.SetActive(false);
        }

        // Aim UI생성해주고 셋팅
        IEnumerator AimSetting(GameObject runObject, GameObject target)
        {
            var aim = Instantiate(aimObject, aimCanvas.transform);
            aim.SetActive(true);
            _aimDictionary.TryAdd(target, aim);

            // Aim Update
            StartCoroutine(AimUpdate(runObject, target, aim));

            // Targeting 애니메이션 기다린 후 회전 애니메이션 실행
            yield return _aimTargetingAniTime;

            var animation = aim.GetComponent<Animation>();
            animation.Play("Clean Shoot Aim Rotation");
        }

        // 생성된 Aim UI를 Target의 위치에 따라 Update
        IEnumerator AimUpdate(GameObject runObject, GameObject target, GameObject aim)
        {
            var rangeHalf = range / 2f;
            var screen = new Vector2(Screen.width, Screen.height);
            var screenHalf = screen / 2f;
            var areaMin = (screenHalf - rangeHalf) / screen;
            var areaMax = (screenHalf + rangeHalf) / screen;

            var aimRect = aim.GetComponent<RectTransform>();
            var camera = ownerPlayer.cameraController.targetCamera;

            while (true)
            {
                yield return null;
                if (isInvoke == false)
                {
                    break;
                }

                Vector3 monsterViewportPosition = camera.WorldToViewportPoint(target.transform.position);
                if (monsterViewportPosition.x >= areaMin.x && monsterViewportPosition.x <= areaMax.x &&
                    monsterViewportPosition.y >= areaMin.y && monsterViewportPosition.y <= areaMax.y &&
                    monsterViewportPosition.z > 0)
                {
                    var dir = (target.transform.position - runObject.transform.position);
                    var aimPos = monsterViewportPosition * screen - (Vector2)aimCanvas.transform.position;
                    var dirMagnitudeNormalize = Mathf.Clamp((50 - dir.magnitude) / 50, 0, 1);
                    var aimScale = new Vector3(dirMagnitudeNormalize, dirMagnitudeNormalize, 1f);
                    aimRect.anchoredPosition = aimPos;
                    aimRect.localScale = aimScale;
                }
                else
                {
                    _aimDictionary.Remove(target);
                    Destroy(aim);
                    break;
                }
            }
        }

        #region RPC Function

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public async void SpawnTrajectoryRPC(NetworkId monsterId, Vector3 viewPosition)
        {
            var monster = Runner.FindObject(monsterId);
            var dis = Vector3.Magnitude(monster.transform.position - viewPosition);
            var trajectoryVFXObject = await NetworkManager.Runner.SpawnAsync(trajectoryVFX, viewPosition);
            var vfx = trajectoryVFXObject.GetComponent<VisualEffect>();

            vfx.SetFloat("Distance", dis);
            vfx.transform.LookAt(monster.transform);
            Destroy(vfx.gameObject, _trajectoryVFXDestroyTime);
        }

        #endregion
    }
}