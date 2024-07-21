using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Loading
{
    public class LobbyLoadingUI : MonoBehaviour
    {
        public GameObject rotateTarget;
        public TMP_Text loadingExplain;
        
        private void Awake()
        {
            EventSystem es = GetComponentInChildren<EventSystem>();
            if(es != null) es.gameObject.SetActive(false);

            LoadingManager.LoadingProcessSuccess += (process) => loadingExplain.text = process;
        }

        private void Update()
        {
            rotateTarget.transform.Rotate(0,0,360 * Time.deltaTime);
        }
    }
}