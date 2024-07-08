using System;
using System.Collections.Generic;
using Status;
using UnityEngine;

namespace GamePlay
{
    public class Difficult : MonoBehaviour
    {
        #region Static

        // Difficult Data 캐싱
        private static readonly Dictionary<string, StatusJsonData> DifficultDataChasing = new Dictionary<string, StatusJsonData>();
        public static void AddDifficultData(string difficultName, StatusJsonData data) => DifficultDataChasing.TryAdd(difficultName, data);
        public static StatusJsonData GetDifficultData(string difficultName) => DifficultDataChasing.TryGetValue(difficultName, out var data) ? data : new StatusJsonData();
        public static void ClearSDifficultData() => DifficultDataChasing.Clear();

        public static void InitDifficult(string diffName)
        {
            if (LanguageDictionary.TryGetValue(diffName, out string diff))
            {
                var difficultData = GetDifficultData(diff);

                MonsterSpawnCountRate = difficultData.GetFloat("Monster Spawn Count Rate");
                MonsterKillCountRate = difficultData.GetFloat("Monster Kill Count Rate");
                AliveMonsterCountRate = difficultData.GetFloat("Alive Monster Count Rate");
                MonsterHpRate = difficultData.GetFloat("Monster Hp Rate");
                MonsterDamageRate = difficultData.GetFloat("Monster Damage Rate");
            }
        }
        public static void AddDifficultLanguage(string language, string english) => LanguageDictionary.Add(language, english);
        
        /// <summary>
        /// 왼쪽이 다른 나라 언어
        /// 오른쪽이 영어
        /// </summary>
        private static Dictionary<string, string> LanguageDictionary = new Dictionary<string, string>();
        
        public static float MonsterSpawnCountRate;
        public static float MonsterKillCountRate;
        public static float AliveMonsterCountRate;

        public static float MonsterHpRate;
        public static float MonsterDamageRate;

        #endregion
        
        private void Awake()
        {
            LanguageDictionary.Clear();
            DontDestroyOnLoad(gameObject);
            AddDifficultLanguage("쉬움", "easy");
            AddDifficultLanguage("보통", "normal");
            AddDifficultLanguage("어려움", "hard");
        }
    }
}