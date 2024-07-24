using UnityEngine;
using Util;

namespace Manager
{
    public class UIManager : Singleton<UIManager>
    {
        #region Static
        public static void AddActiveUI(GameObject uiObject)
        {
            Instance._ActiveUIQueue.Enqueue(new [] {uiObject});
            Instance._isAdd = true;
        }
        public static void AddActiveUI(GameObject[] uiObjects)
        {
            Instance._ActiveUIQueue.Enqueue(uiObjects);
            Instance._isAdd = true;
        }

        public static GameObject[] Dequeue() => Instance._ActiveUIQueue.Dequeue();
        
        public static bool HasActiveUI() => Instance._ActiveUIQueue.IsEmpty() == false || Instance._isAdd;

        #endregion

        private UniqueQueue<GameObject[]> _ActiveUIQueue;
        private bool _isAdd; // 현재 프레임에 추가된 UI가 있는지
        
        private void Awake()
        {
            base.Awake();
            _ActiveUIQueue = new UniqueQueue<GameObject[]>();
        }

        public void LateUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && _isAdd == false)
            {
                if (_ActiveUIQueue.IsEmpty() == false)
                {
                    var queue = _ActiveUIQueue.Dequeue();
                    foreach (var o in queue)
                        if(o) o.SetActive(false);
                }
            }

            _isAdd = false;
        }
        
        public static void ActiveUIAllDisable()
        {
            var list = Instance._ActiveUIQueue.AllDequeue();
            foreach (var uiObjs in list)
            {
                foreach (var uiObj in uiObjs)
                {
                    if(uiObj) uiObj.SetActive(false);
                }
            }
        }

        public static void QueueClear() => Instance._ActiveUIQueue.AllDequeue();
    }
}