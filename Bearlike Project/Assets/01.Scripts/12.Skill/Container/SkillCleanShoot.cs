using System.Collections;
using System.Collections.Generic;
using Manager;
using Monster;
using State.StateClass;
using State.StateClass.Base;
using UnityEngine;

namespace Skill.Container
{
    public class SkillCleanShoot : SkillBase
    {
        public GameObject areaObject;
        public Vector2 range;

        private RectTransform _areaRect;
        private Animation areaAnimation;
        private WaitForSeconds _areaOpenAniTime; // UI 키는 애니메이션 시간
        private LayerMask _layerMask;

        private WaitForSeconds _findTime; // 몇초를 주기로 영역내에 몬스터 포착 업데이트를 하게 할지

        private List<MonsterBase> _monsterList = new List<MonsterBase>(); // 타격할 몬스터를 담는 컨테이너
        private bool _isAttack;

        private void Awake()
        {
            areaAnimation = areaObject.GetComponent<Animation>();
            var clip = areaAnimation.GetClip("Open Clean Shoot Area");
            _areaOpenAniTime = new WaitForSeconds(clip.length);
            _areaRect = areaObject.GetComponent<RectTransform>();
            _layerMask = LayerMask.GetMask("Default");

            _findTime = new WaitForSeconds(0.2f);
        }

        public override void MainLoop()
        {
            if (coolTime.isMin == false)
            {
                coolTime.Current -= Time.deltaTime;
            }
        }

        public override void Run(GameObject runObject)
        {
            if (coolTime.isMin && isInvoke == false)
            {
                isInvoke = true;
                gameObject.SetActive(true);
                _areaRect.sizeDelta = range;
                StartCoroutine(SettingArea(runObject));
                StartCoroutine(AttackMonsterFromArea());
            }
        }

        IEnumerator AttackMonsterFromArea()
        {
            yield return _areaOpenAniTime;
            while (true)
            {
                yield return null;
                if (KeyManager.InputAction(KeyToAction.Attack))
                {
                    foreach (var monster in _monsterList)
                    {
                        var status = monster.GetComponent<MonsterStatus>();
                        status.ApplyDamageRPC(damage.Current, CrowdControl.Normality);
                    }

                    // 스킬이 끝난 뒤에 초기화 해주기
                    gameObject.SetActive(false);
                    isInvoke = false;
                    coolTime.Current = coolTime.Max;
                    break;
                }
            }
        }

        IEnumerator SettingArea(GameObject runObject)
        {
            // viewport상의 영역 크기 잡아주기
            var rangeHalf = range / 2f;
            var screen = new Vector2(Screen.width, Screen.height);
            var screenHalf = screen / 2f;
            var areaMin = (screenHalf - rangeHalf) / screen;
            var areaMax = (screenHalf + rangeHalf) / screen;

            // 영역 여는 애니메이션 길이만큼 대기
            areaAnimation.Play();
            yield return _areaOpenAniTime;

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
                        DebugManager.DrawRay(monsterPosition, dir, Color.blue, 3f);
                        if (Physics.Raycast(monsterPosition, dir, out var hit, _layerMask) == false)
                        {
                            _monsterList.Add(monster);
                        }
                    }
                }
            }
        }
    }
}