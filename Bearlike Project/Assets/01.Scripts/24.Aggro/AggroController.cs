using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Aggro
{
    public class AggroController : MonoBehaviour
    {
        private readonly HashSet<AggroTarget> _aggroList = new HashSet<AggroTarget>(); // 어그로 타깃이 될 수 있는 대상들
        [SerializeField] private AggroTarget target; // 어그로 대상
        [SerializeField] private int targetScore; // 현재 어그로 대상의 스코어
        [SerializeField] private float aggroRange; // 어그로 범위
        private Func<int> _aggroScoreFunc;

        #region Member Function

        public bool HasTarget() => target;
        public AggroTarget GetTarget() => target;

        public void AddTarget(AggroTarget obj) => _aggroList.Add(obj);
        public void AddTarget(AggroTarget[] objs) => _aggroList.AddRange(objs);
        public void RemoveTarget(AggroTarget obj) => _aggroList.Remove(obj);

        public void AddScoreFunc(Func<int> func) => _aggroScoreFunc += func;
        public void RemoveScoreFunc(Func<int> func) => _aggroScoreFunc -= func;

        public float GetRange() => aggroRange;
        public void SetRange(float value) => aggroRange = value;

        public void ChangeAggroTarget(AggroTarget obj, bool isCompareScore = false)
        {
            var score = CalAggroScore(obj);
            if (isCompareScore)
            {
                if (score > targetScore)
                {
                    targetScore = score;
                    target = obj;
                }
            }
            else
            {
                targetScore = score;
                target = obj;
            }
        }

        public AggroTarget FindAggroTarget(bool isCheckDis = true)
        {
            foreach (var aggroTarget in _aggroList)
            {
                var value = aggroTarget.CheckAggro();
                if (value)
                {
                    if (isCheckDis && Vector3.Distance(aggroTarget.transform.position, transform.position) > aggroRange) continue;

                    var score = CalAggroScore(aggroTarget);
                    if (score > targetScore)
                    {
                        target = aggroTarget;
                        targetScore = score;
                    }
                }
            }

            return target;
        }

        public void CheckTargetAggro()
        {
            if (!target) return;
            if (!target.CheckAggro())
            {
                target = null;
                targetScore = -1;
            }
        }

        public bool CheckTargetDistance()
        {
            if (!target) return false;
            if (Vector3.Distance(target.transform.position, transform.position) > aggroRange)
            {
                target = null;
                return false;
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aggroTarget"></param>
        /// <returns></returns>
        private int CalAggroScore(AggroTarget aggroTarget)
        {
            int score = aggroTarget.CalAggroScore();

            if (_aggroScoreFunc != null)
            {
                foreach (var @delegate in _aggroScoreFunc.GetInvocationList())
                {
                    var func = (Func<int>)@delegate;
                    score += func();
                }
                
            }

            return score;
        }

        #endregion
    }
}