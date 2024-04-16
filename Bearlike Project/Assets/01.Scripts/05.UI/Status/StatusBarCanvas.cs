using System.Collections.Generic;
using UnityEngine;
using Util;

namespace UI.Status
{
    public class StatusBarCanvas : Singleton<StatusBarCanvas>
    {
        public Canvas canvas;
        public GameObject hpBarSliderPrefab;

        public float visibleDistance = 10f; // Bar가 보이는 거리

        public List<StatusBarUI> barList = new List<StatusBarUI>();
        
        private Camera _camera;

        private void FixedUpdate()
        {
            if (_camera == null || _camera.gameObject.activeSelf == false)
                _camera = Camera.main;
            
            foreach (var statusBarUI in barList)
            {
                statusBarUI.SetBarActive((Vector3.Distance(statusBarUI.transform.position, _camera.transform.position) < visibleDistance) && IsBehindCamera(_camera, statusBarUI.transform));
                statusBarUI.BarUpdate(_camera);
            }
        }
        
        private bool IsBehindCamera(Camera camera, Transform target)
        {
            Vector3 toTarget = (target.position - camera.transform.position).normalized;  // 카메라에서 대상까지의 방향 벡터
            float dotProduct = Vector3.Dot(camera.transform.forward, toTarget);  // 카메라 전방 벡터와 대상 방향 벡터의 내적 계산

            return dotProduct < 0;  // 내적 결과가 0보다 작으면 true 반환
        }
    }
}

