using System;
using System.Collections;
using System.Collections.Generic;
using Manager;
using State.StateClass;
using State.StateClass.Base;
using Unity.Mathematics;
using UnityEngine;

namespace Skill.Container
{
    public class SkillCleanShoot : SkillBase
    {
        public GameObject areaObject;

        private Animation areaAnimation;
        private WaitForSeconds _areaOpenAniTime;
        
        private void Awake()
        {
            areaAnimation = areaObject.GetComponent<Animation>();
            var clip = areaAnimation.GetClip("Open Clean Shoot Area");
            _areaOpenAniTime = new WaitForSeconds(clip.length);
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
                StartCoroutine(SettingArea(runObject));
            }
        }

        IEnumerator SettingArea(GameObject runObject)
        {
            // 영역 여는 애니메이션 길이만큼 대기
            areaAnimation.Play();
            yield return _areaOpenAniTime;

            Vector3 center = Camera.main.transform.position;
            LayerMask monsterLayer = 1 << LayerMask.NameToLayer("Monster");
            RaycastHit[] hits = Physics.BoxCastAll(center, new Vector3(500, 500, float.MaxValue), runObject.transform.forward, quaternion.identity, monsterLayer);

            // Monster들의 앞에 장애물이 있는지 체크
            List<GameObject> monsterList = new List<GameObject>();
            var boxHitMaks = 1 << LayerMask.NameToLayer("Player") | 1 << LayerMask.NameToLayer("Default");
            foreach (var boxHit in hits)
            {
                var dir = boxHit.point - center;
                DebugManager.DrawRay(boxHit.point, dir * float.MaxValue, Color.red, 3f);
                if (Physics.Raycast(boxHit.point, dir, out var hit, float.MaxValue, boxHitMaks))
                {
                    if (hit.transform.CompareTag("Player") == false) continue;
                    if (IsFront(runObject.transform, hit.transform))
                    {
                        monsterList.Add(hit.transform.gameObject);
                    }
                }
            }

            DebugManager.ToDo("영역에 잡힌 Monster들의 위치를 UI로 띄어주고 일정 시간 이후에 대미지 입히기");

            foreach (var monster in monsterList)
            {
                var status = monster.GetComponent<MonsterStatus>();
                status.ApplyDamageRPC(damage.Current, CrowdControl.Normality);
            }

            // 스킬이 끝난 뒤에 초기화 해주기
            gameObject.SetActive(false);
            coolTime.Current = coolTime.Max;
            isInvoke = false;
        }
        
        public bool IsFront(Transform observer, Transform target)
        {
            Vector3 toTarget = (target.position - observer.position).normalized;
            float dotProduct = Vector3.Dot(observer.forward, toTarget);

            return dotProduct > 0;
        }
    }
}