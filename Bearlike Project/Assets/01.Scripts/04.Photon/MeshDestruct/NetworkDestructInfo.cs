using Fusion;
using UnityEngine;

namespace Photon.MeshDestruct
{
    public struct NetworkSliceInfo : INetworkStruct
    {
        public NetworkId TargetId;
        
        public NetworkId SliceID0;
        public NetworkId SliceID1;
        
        public Vector3 SliceNormal;
        public Vector3 SlicePoint;
    }
}