using UnityEngine;

namespace Script.Util
{
    public class ObjectUtil
    {
        /// <summary>
        /// if not have Tpye(T) Component, _Object do Add Component
        /// </summary>
        public static T GetORAddComponet<T>(GameObject _Object) where T : UnityEngine.Component
        {
            T component = _Object.GetComponent<T>();
            if (component == null)
                component = _Object.AddComponent<T>();

            return component;
        }

        public static GameObject FindChild(GameObject _Object, string name = null, bool isRecursive = false)
        {
            Transform transform = FindChild<Transform>(_Object, name, isRecursive);
            if (transform == null)
            {
                Debug.LogWarning($"{_Object.name}하위에 {name}이라는 오브젝트가 존재하지 않습니다.");
                return null;
            }

            return transform.gameObject;
        }

        public static T FindChild<T>(GameObject _Object, string name = null, bool isRecursive = false) where T : UnityEngine.Object
        {
            if (_Object == null)
                return null;

            if (isRecursive == false)
            {
                for (int i = 0; i < _Object.transform.childCount; i++)
                {
                    Transform trasform = _Object.transform.GetChild(i);
                    if (string.IsNullOrEmpty(name) || trasform.name == name)
                    {
                        T component = trasform.GetComponent<T>();
                        if (component != null)
                            return component;
                    }
                }
            }
            else
            {
                foreach (T componen in _Object.GetComponentsInChildren<T>())
                {
                    if (string.IsNullOrEmpty(name) || componen.name == name)
                        return componen;
                }
            }

            return null;
        }

        // 생성하려는 오브젝트가 존재하지 않으면 생성해주지 않음
        // _Object = 생성할 오브젝트
        // parant = 생성한 객체의 부모 객체
        public static T Instantiate<T>(T _Object, Transform parant = null) where T : UnityEngine.Object
        {
            if (_Object == null)
            {
                Debug.LogWarning($"Falied Instantiate : {typeof(T)}");
                return null;
            }

            return UnityEngine.Object.Instantiate(_Object, parant);
        }
        public static GameObject[] GetChildren(GameObject _Object)
        {
            GameObject[] objs = new GameObject[_Object.transform.childCount];
            int i = 0;
            foreach (var transform in GetChildren<Transform>(_Object))
                objs[i++] = transform.gameObject;

            return objs;
        }

        public static T[] GetChildren<T>(GameObject _Object) where T : UnityEngine.Object
        {
            T[] children = new T[_Object.transform.childCount];
            for (int i = 0; i < _Object.transform.childCount; i++)
                children[i] = _Object.transform.GetChild(i).GetComponent<T>();
        
            return children;
        }

        //public static void Swap<T>(this List<T> list, int from, int to)
        //{
        //    T tmp = list[from];
        //    list[from] = list[to];
        //    list[to] = tmp;
        //}
    }
}