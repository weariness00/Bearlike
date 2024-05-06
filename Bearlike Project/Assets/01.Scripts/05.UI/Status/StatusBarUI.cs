using Manager;
using Monster;
using Status;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Status
{
    public class StatusBarUI : MonoBehaviour
    {
        private StatusBase _status;

        private Slider _hpBarSlider;

        
        private Vector3 headPosition; // 진짜 머리 위 포지션은 아니고 Mesh로 계산했을떄의 머리 위를 말함

        private void Awake()
        {
            _status = GetComponent<StatusBase>();
            
            if(_status == null)
                DestroyImmediate(this);

            if (TryGetComponent(out MonsterBase mb))
            {
                mb.DieAction += () => Destroy(this);
            }

            var meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
            var skinnedMeshes = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            float height = 0;
            foreach (var meshFilter in meshFilters)
            {
                var bound = meshFilter.sharedMesh.bounds;
                height = (bound.max.y - bound.min.y);
                if (headPosition.y > height)
                    height = headPosition.y;
            }
            foreach (var skinnedMesh in skinnedMeshes)
            {
                var bound = skinnedMesh.bounds;
                height = (bound.max.y - bound.min.y) + 0.2f;
                if (headPosition.y > height)
                    height = headPosition.y;
            }
            headPosition = new Vector3(0, height, 0) * transform.localScale.y;

        }

        private void Start()
        {
            _hpBarSlider = Instantiate(StatusBarCanvas.Instance.hpBarSliderPrefab, StatusBarCanvas.Instance.canvas.transform).GetComponent<Slider>();
            
            StatusBarCanvas.Instance.barList.Add(this);

            SetBarActive(false);
        }

        private void OnDestroy()
        {
            StatusBarCanvas.Instance.barList.Remove(this);
            if(_hpBarSlider) Destroy(_hpBarSlider.gameObject);
        }

        public void BarUpdate(Camera c)
        {
            if (_hpBarSlider.gameObject.activeSelf)
            {
                DebugManager.ToDo("거리에 따라 Bar 크기를 작게 해야된다.");
       
                _hpBarSlider.value = (float)_status.hp.Current / _status.hp.Max;
                _hpBarSlider.transform.position = c.WorldToScreenPoint(gameObject.transform.position + headPosition);
            }
        }

        public void SetBarActive(bool value)
        {
            _hpBarSlider.gameObject.SetActive(value);
        }
    }
}

