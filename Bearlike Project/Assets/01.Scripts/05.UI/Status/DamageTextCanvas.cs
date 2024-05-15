using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Util;

namespace UI.Status
{
    public class DamageTextCanvas : Singleton<DamageTextCanvas>
    {
        private Canvas _canvas;
        public TMP_Text damageText;

        private Camera _camera;
        private List<Tuple<Vector3,TMP_Text>> _damageTextList = new List<Tuple<Vector3,TMP_Text>>();

        protected override void Awake()
        {
            base.Awake();
            _canvas = GetComponent<Canvas>();
        }

        private void Update()
        {
            for (var i = 0; i < _damageTextList.Count; i++)
            {
                var (originPosition, dt) = _damageTextList[i];
                if (!dt)
                {
                    _damageTextList.RemoveAt(i);
                    continue;
                }
                dt.gameObject.SetActive(!IsBehindCamera(_camera, originPosition));
                if(!dt.gameObject.activeSelf) continue;
                
                float dir = Vector3.Distance(originPosition, _camera.transform.position);
                float dtScale = 1f;
                if (dir > 5f) dtScale = 5f / dir * _canvas.scaleFactor;

                dt.rectTransform.localScale = new Vector3(dtScale, dtScale, dtScale);
                dt.rectTransform.position = _camera.WorldToScreenPoint(originPosition);
            }
        }

        public static void SpawnDamageText(Vector3 position, int damage) => Instance.SpawnDamageText_(position, damage);
        
        private void SpawnDamageText_(Vector3 position, int damage)
        {
            if (!_camera)
            {
                _camera = Camera.main;
                _canvas.worldCamera = _camera;
            }
            
            var dtObj = Instantiate(damageText.gameObject, transform);
            var dt = dtObj.GetComponent<TMP_Text>();
            dt.text = damage.ToString();
            dt.DOFade(0, 3f);
            
            _damageTextList.Add(new Tuple<Vector3, TMP_Text>(position, dt));
            Destroy(dtObj, 3f);
        }
        
        private bool IsBehindCamera(Camera camera, Vector3 originPosition)
        {
            Vector3 toTarget = (originPosition - camera.transform.position).normalized;  // 카메라에서 대상까지의 방향 벡터
            float dotProduct = Vector3.Dot(camera.transform.forward, toTarget);  // 카메라 전방 벡터와 대상 방향 벡터의 내적 계산

            return dotProduct < 0;  // 내적 결과가 0보다 작으면 true 반환
        }
    }
}

