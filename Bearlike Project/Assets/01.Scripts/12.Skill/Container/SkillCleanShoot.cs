using System;
using System.Collections;
using System.Collections.Generic;
using Manager;
using Monster;
using State.StateClass;
using State.StateClass.Base;
using Unity.Mathematics;
using UnityEngine;

namespace Skill.Container
{
    public class SkillCleanShoot : SkillBase
    {
        public GameObject areaObject;
        public Vector2 range;

        private RectTransform _areaRect;
        private Animation areaAnimation;
        private WaitForSeconds _areaOpenAniTime;
        private LayerMask _layerMask;
        
        private void Awake()
        {
            areaAnimation = areaObject.GetComponent<Animation>();
            var clip = areaAnimation.GetClip("Open Clean Shoot Area");
            _areaOpenAniTime = new WaitForSeconds(clip.length);
            _areaRect = areaObject.GetComponent<RectTransform>();
            _layerMask = 1 << LayerMask.NameToLayer("Player") | 1 << LayerMask.NameToLayer("Default");
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

            DebugManager.ToDo("영역에 잡힌 Monster들의 위치를 UI로 띄어주고 일정 시간 이후에 대미지 입히기");

            var monsters = FindObjectsOfType<MonsterBase>();

            foreach (var monster in monsters)
            {
                Vector3 monsterViewportPosition = Camera.main.WorldToViewportPoint(monster.transform.position);

                // monsterViewportPosition의 값이 areaMin, areaMax 값 사이에 있으면 객체가 UI 영역 내에 있다고 판단할 수 있습니다.
                if (monsterViewportPosition.x >= areaMin.x && monsterViewportPosition.x <= areaMax.x &&
                    monsterViewportPosition.y >= areaMin.y && monsterViewportPosition.y <= areaMax.y &&
                    monsterViewportPosition.z > 0)
                {
                    var dir = runObject.transform.position - monster.transform.position;
                    DebugManager.DrawRay(monster.transform.position, dir, Color.red, 3f);
                    if (Physics.Raycast(monster.transform.position, dir, out var hit, _layerMask) &&
                        hit.transform.CompareTag("Player"))
                    {
                        var status = monster.GetComponent<MonsterStatus>();
                        status.ApplyDamageRPC(damage.Current, CrowdControl.Normality);
                    }
                }
            }

            // 스킬이 끝난 뒤에 초기화 해주기
            gameObject.SetActive(false);
            coolTime.Current = coolTime.Max;
            isInvoke = false;
        }
    }
}