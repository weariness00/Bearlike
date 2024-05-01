using Fusion;
using UnityEngine;

namespace Photon.MeshDestruct
{
    public struct NetworkDestructInfo : INetworkStruct
    {
        public int Id; // 어떤 객체를 자를 것인지에 대한 ID 이 ID로 Object를 탐색한다.
        public PrimitiveType ShapeType;
        public Vector3 ShapePosition;
        public Vector3 ShapeSize;

        public Vector3 SliceNormal;
        public NetworkId SliceObjectId0;
        public NetworkId SliceObjectId1;

        public NetworkId InteractObjectId; // Is Slice가 false 경우 부서진 객체 정보 수신
    }

    public struct NetworkSliceInfo : INetworkStruct
    {
        public NetworkId TargetId;
        
        public NetworkId SliceID0;
        public NetworkId SliceID1;
        
        public Vector3 SliceNormal;
        public Vector3 SlicePoint;
    }
}