// using System;
// using System.Collections.Generic;
// using GamePlay.StageLevel;
// using UnityEngine;
// using UnityEngine.Serialization;
// using Util;
//
// namespace Monster
// {
//     public class MonsterManager : Singleton<MonsterManager>
//     {
//         public List<string> monsterList;
//         public Dictionary<string, int> MonsterCountDictionary;
//
//         private void Awake()
//         {
//             monsterList = new List<string>();
//             MonsterCountDictionary = new Dictionary<string, int>();
//             
//             monsterList.Add("TrumpCard");
//             monsterList.Add("PiggyBank");
//
//             foreach (var monsterName in monsterList)
//             {
//                 MonsterCountDictionary.Add(monsterName, 0);
//             }
//         }
//
//         private void Update()
//         {
//             foreach (var pair in MonsterCountDictionary)
//             {
//                 Debug.Log($"{pair.Key} : {pair.Value}마리");
//             }
//             
//         }
//     }
// }