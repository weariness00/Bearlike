using System;
using System.Collections.Generic;
using System.Linq;

namespace Util
{
    public class UniqueQueue<T>
    {
        private Queue<T> queue = new Queue<T>();
        private HashSet<T> hashSet = new HashSet<T>();

        // 요소 추가
        public void Enqueue(T item)
        {
            // HashSet에 요소가 이미 존재하지 않는 경우에만 추가
            if (hashSet.Add(item))
            {
                queue.Enqueue(item);
            }
        }

        // 요소 제거 및 반환
        public T Dequeue()
        {
            if (queue.Count > 0)
            {
                T item = queue.Dequeue();
                hashSet.Remove(item);
                return item;
            }
            throw new InvalidOperationException("Queue is empty");
        }

        // 모든 요소 제거 및 반환
        public List<T> AllDequeue()
        {
            var list = queue.ToList();
            queue.Clear();
            hashSet.Clear();
            return list;
        }

        // 큐가 비어 있는지 확인
        public bool IsEmpty()
        {
            return queue.Count == 0;
        }
    }
}