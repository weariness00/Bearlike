using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Util.Map
{
    public class MapGenerate
    {
        private readonly Vector3[] directions = { Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
        
        public bool isAheadGenerate = false;
        public Vector3 currentPos;

        public HashSet<MapInfo> generatedMapInfoList = new HashSet<MapInfo>();
        
        #region Util Function

        private Vector3 GetRandomDirection()
        {
            // 랜덤한 인덱스 선택
            int randomIndex = Random.Range(0, directions.Length);
            // 선택된 방향 반환
            return directions[randomIndex];
        }

        private Vector3 DirectionMultiple(Vector3 a, Vector3 b)
        {
            return new Vector3(
                a.x * b.x,
                a.y * b.y,
                a.z * b.z
            );
        }
        
        #endregion
        
        /// <summary>
        /// 빈공간을 찾아 반환해주는 재귀적 함수
        /// </summary>
        /// <param name="mapInfo"> 빈 공간에 들어갈 맵 정보 </param>
        /// <param name="generatedMapInfo"> 이미 생성된 맵 정보 없으면 Default값 </param>
        /// <returns></returns>
        public async Task<MapInfo> FindEmptySpace(MapInfo mapInfo, MapInfo generatedMapInfo = default)
        {
            var dir = GetRandomDirection();
            var origin = generatedMapInfo.pivot + DirectionMultiple(dir / 2, generatedMapInfo.size);
            MapInfo makeMapInfo;
            
            if (Physics.Raycast(origin, dir * int.MaxValue, out var hit, LayerMask.GetMask("Map")))
            {
                generatedMapInfo = hit.collider.GetComponent<MapInfoMono>().info;
                makeMapInfo = await FindEmptySpace(mapInfo, generatedMapInfo);
            }
            else
            {
                makeMapInfo.pivot = origin + DirectionMultiple(dir / 2, mapInfo.size);
                makeMapInfo.size = mapInfo.size;
            }

            return makeMapInfo;
        }

        public void AddMap(MapInfo info) => generatedMapInfoList.Add(info);
        public void RemoveMap(MapInfo info) => generatedMapInfoList.Remove(info);
    }
}