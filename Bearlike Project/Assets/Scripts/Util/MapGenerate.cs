using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Util
{
    public class MapGenerate
    {
        private readonly Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
        
        public bool isAheadGenerate = false;
        public Vector3 currentPos;

        public List<MapInfo> generatedMapInfoList = new List<MapInfo>();
        
        #region struct

        [System.Serializable]
        public struct MapInfo
        {
            public Vector3 pivot;
            public Vector3 size;
        }

        #endregion

        #region Util Function

        private Vector3 GetRandomDirection()
        {
            // 랜덤한 인덱스 선택
            int randomIndex = Random.Range(0, directions.Length);
            // 선택된 방향 반환
            return directions[randomIndex];
        }
        
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tuple"> First : Map Size, Second : Map Object</param>
        /// <returns></returns>
        public IEnumerator Generate(List<Tuple<Vector3, GameObject>> tupleList)
        {
            foreach (var (mapSize, mapObject) in tupleList)
            {
                yield return null;
            }
        }

        public async Task<MapInfo> Generate(MapInfo generatedMapInfo = default)
        {
            var dir = GetRandomDirection();
            var origin = generatedMapInfo.pivot + new Vector3(dir.x * generatedMapInfo.size.x, dir.y * generatedMapInfo.size.y, dir.z * generatedMapInfo.size.z);
            MapInfo makeMapInfo;
            
            if (Physics.Raycast(origin, dir * int.MaxValue, out var hit, LayerMask.GetMask("Map")))
            {
                makeMapInfo = await Generate( generatedMapInfo);
            }

            return makeMapInfo;
        }
    }
}