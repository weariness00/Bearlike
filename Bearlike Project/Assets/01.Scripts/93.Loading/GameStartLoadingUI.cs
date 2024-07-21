using System;
using System.Collections;
using GamePlay;
using Photon;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Loading
{
    public class GameStartLoadingUI : MonoBehaviour
    {
        public Slider loadingBar;
        private RectTransform _barHandle;

        private Coroutine loadingCoroutine;
        
        private void Awake()
        {
            EventSystem es = GetComponentInChildren<EventSystem>();
            if(es != null) es.gameObject.SetActive(false);

            loadingBar.value = 0;
            _barHandle = loadingBar.handleRect;
            
            LoadingManager.Initialize();
            LoadingManager.StartAction += () =>
            {
                loadingCoroutine ??= StartCoroutine(LoadingCoroutine());
            };
            LoadingManager.EndAction += () =>
            {
                if(loadingCoroutine != null) StopCoroutine(loadingCoroutine);
                GameManager.Instance.isControl = true;
                SceneManager.UnloadSceneAsync(gameObject.scene.path);
            };
        }

        private void Update()
        {
            _barHandle.Rotate(0,0, 30);
        }

        IEnumerator LoadingCoroutine()
        {
            GameManager.Instance.isControl = false;
            
            var refValue = LoadingManager.Instance.refValue;
            
            while (true)
            {
                loadingBar.value = (float)refValue.Current / refValue.Max;

                yield return null;
            }
        }
    }
}