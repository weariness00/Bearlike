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
                var monsterLayer = LayerMask.NameToLayer("Monster");
            }
        }
    }
}