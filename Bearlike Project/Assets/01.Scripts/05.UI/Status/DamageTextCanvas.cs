using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Util;

namespace UI.Status
{
    public enum DamageTextType
    {
        Normal,
        Critical,
        Heal,
    }
    
    public class DamageTextCanvas : Singleton<DamageTextCanvas>
    {
        private Canvas _canvas;
        public TMP_Text normalDamageText;
        public TMP_Text criticalDamageText;
        public TMP_Text healText;

        private Camera _camera;
        private List<DamageTextInfo> _damageTextList = new List<DamageTextInfo>();

        protected override void Awake()
        {
            base.Awake();
            _canvas = GetComponent<Canvas>();
        }

        private void Update()
        {
            for (var i = 0; i < _damageTextList.Count; i++)
            {
                DamageTextInfo dtInfo = _damageTextList[i];
                if (!dtInfo.text)
                {
                    _damageTextList.RemoveAt(i);
                    continue;
                }
                
                // 카메라 뒤에 있으면 off
                dtInfo.text.gameObject.SetActive(!IsBehindCamera(_camera, dtInfo.originPosition));
                if(!dtInfo.text.gameObject.activeSelf) continue;
                
                // 거리에 따라 scale 조정
                float dir = Vector3.Distance(dtInfo.originPosition, _camera.transform.position);
                float dtScale = 1f;
                if (dir > 5f) dtScale = 5f / dir * _canvas.scaleFactor;

                // ui상의 위치 업데이트
                dtInfo.text.rectTransform.localScale = new Vector3(dtScale, dtScale, dtScale) * dtInfo.multipleScale;
                dtInfo.text.rectTransform.position = _camera.WorldToScreenPoint(dtInfo.originPosition);
                dtInfo.originPosition += Vector3.up * Time.deltaTime;
            }
        }

        private TMP_Text GetDamageText(DamageTextType type)
        {
            if (type == DamageTextType.Normal)
                return normalDamageText;
            if (type == DamageTextType.Critical)
                return criticalDamageText;
            if (type == DamageTextType.Heal)
                return healText;

            return null;
        }

        public static void SpawnDamageText(Vector3 position, int damage, DamageTextType type = DamageTextType.Normal) => Instance.SpawnDamageText_(position, damage, type);
        
        private void SpawnDamageText_(Vector3 position, int damage, DamageTextType type)
        {
            if (!_camera)
            {
                _camera = Camera.main;
                _canvas.worldCamera = _camera;
            }
            
            // Damage Text 생성
            var dtObj = Instantiate(GetDamageText(type).gameObject, transform);
            var dt = dtObj.GetComponent<TMP_Text>();
            dtObj.SetActive(true);
            dt.text = type == DamageTextType.Heal ? "+" + damage : damage.ToString();
            dt.DOFade(0, 3f); // Alpha 감소

            // dt 생성
            var dtInfo = new DamageTextInfo() { originPosition = position, text = dt };
            if (type == DamageTextType.Critical) dtInfo.multipleScale = 1.5f;
            
            _damageTextList.Add(dtInfo);
            Destroy(dtObj, 3f);
        }
        
        private bool IsBehindCamera(Camera camera, Vector3 originPosition)
        {
            Vector3 toTarget = (originPosition - camera.transform.position).normalized;  // 카메라에서 대상까지의 방향 벡터
            float dotProduct = Vector3.Dot(camera.transform.forward, toTarget);  // 카메라 전방 벡터와 대상 방향 벡터의 내적 계산

            return dotProduct < 0;  // 내적 결과가 0보다 작으면 true 반환
        }

        private class DamageTextInfo
        {
            public float multipleScale = 1f;
            public Vector3 originPosition;
            public TMP_Text text;
        }
    }
}

