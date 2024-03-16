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
        public override void MainLoop()
        {
            if (CoolTime.isMin == false)
            {
                CoolTime.Current -= Time.deltaTime;
            }
        }

        public override void Run(GameObject runObject)
        {
            if (CoolTime.isMin)
            {
                DebugManager.ToDo("Clean Shoot 발동 UI 만들기");
                
                Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
                LayerMask monsterLayer = LayerMask.NameToLayer("Monster");
                RaycastHit[] hits = Physics.BoxCastAll(ray.origin, new Vector3(float.MaxValue, 500, 500), Vector3.zero, quaternion.identity, monsterLayer);
                
                // Monster들의 앞에 장애물이 있는지 체크
                List<GameObject> monsterList = new List<GameObject>();
                var boxHitMaks = LayerMask.NameToLayer("Player") ^ LayerMask.NameToLayer("Default");
                foreach (var boxHit in hits)
                {
                    var dir = boxHit.point - ray.origin;
                    if (Physics.Raycast(boxHit.point, dir, out var hit, float.MaxValue, boxHitMaks))
                    {
                        if(hit.transform.CompareTag("Player") == false) continue;
                        monsterList.Add(hit.transform.gameObject);
                    }
                }
                
                DebugManager.ToDo("영역에 잡힌 Monster들의 위치를 UI로 띄어주고 일정 시간 이후에 대미지 입히기");
                
                foreach (var monster in monsterList)
                {
                    var status = monster.GetComponent<MonsterStatus>();
                    status.ApplyDamageRPC(damage.Current, CrowdControl.Normality);
                }

                CoolTime.Current = CoolTime.Max;
                isInvoke = false;
            }
        }
    }
}