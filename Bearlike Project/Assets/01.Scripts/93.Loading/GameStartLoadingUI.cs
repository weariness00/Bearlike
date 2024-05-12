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
        
        private void Awake()
        {
            EventSystem es = GetComponentInChildren<EventSystem>();
            if(es != null) es.gameObject.SetActive(false);

            loadingBar.value = 0;
            _barHandle = loadingBar.handleRect;
            
            LoadingManager.Initialize();

            StartCoroutine(LoadingCoroutine());
        }

        private void Update()
        {
            _barHandle.Rotate(0,0, 30);
        }

        IEnumerator LoadingCoroutine()
        {
            GameManager.Instance.isControl = false;
            
            yield return new WaitForSeconds(1);

            var refValue = LoadingManager.Instance.refValue;
            
            while (true)
            {
                if (refValue.isMax)
                    break;
                
                loadingBar.value = (float)refValue.Current / refValue.Max;

                yield return null;
            }

            loadingBar.value = 1;
            yield return new WaitForSeconds(0.5f);

            GameManager.Instance.isControl = true;
            SceneManager.UnloadSceneAsync(gameObject.scene.path);
        }
    }
}