using System;
using System.Collections.Generic;
using UnityEngine;

namespace Aggro
{
    public class AggroTarget : MonoBehaviour
    {
        private Func<bool> _conditionFunc;
        private Func<int> _scoreFunc; // AggroController가 사용하는 함수 일종의 가산점

        public void AddCondition(Func<bool> func) => _conditionFunc += func;
        public void RemoveCondition(Func<bool> func) => _conditionFunc -= func;

        public void AddScore(Func<int> func) => _scoreFunc += func;
        public void RemoveScore(Func<int> func) => _scoreFunc -= func;

        /// <summary>
        /// 현재 어그로의 대상이 될 수 있는 상태인지 체크
        /// </summary>
        /// <returns>true : 어그로 가능, false : 어그로 불가능</returns>
        public bool CheckAggro()
        {
            if (_conditionFunc != null)
            {
                foreach (var @delegate in _conditionFunc.GetInvocationList())
                {
                    var func = (Func<bool>)@delegate;
                    var value = func();
                    if (!value) return false;
                }
            }

            return true;
        }

        public int CalAggroScore()
        {
            int score = 0;
            if (_scoreFunc != null)
            {
                foreach (var @delegate in _scoreFunc.GetInvocationList())
                {
                    var func = (Func<int>)@delegate;
                    score += func();
                }
            }
            return score;
        }
    }
}