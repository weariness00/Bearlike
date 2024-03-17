using System;
using System.Collections.Concurrent;
using UnityEngine;
using Util;

namespace GamePlay.Sync
{
    public class TransformSyncSystem : Singleton<TransformSyncSystem>
    {
        private static readonly ConcurrentQueue<Action> tasks = new ConcurrentQueue<Action>();
        
        public static void Enqueue(Action task)
        {
            if (task == null) return;

            tasks.Enqueue(task);
        }

        public static void Dequeue(Action task)
        {
            if (task != null)
            {
            }
        }

        void Update()
        {
            while (tasks.TryDequeue(out var task))
            {
                task?.Invoke();
            }
        }
    }
}

