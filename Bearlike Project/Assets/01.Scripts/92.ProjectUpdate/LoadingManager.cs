using UnityEngine;
using Util;

namespace ProjectUpdate
{
    public class LoadingManager : Singleton<LoadingManager>
    {
        public int refCount = 0;
        public ulong sumByte = 0;

        public static void AddRef(ulong byteLength = 0)
        {
            ++Instance.refCount;
            Instance.sumByte += byteLength;
        }
    }
}

