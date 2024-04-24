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
                Destroy(gameObject);

            if (TryGetComponent(out MonsterBase mb))
            {
                mb.DieAction += () => Destroy(gameObject);
            }

            var mesh = gameObject.GetComponentsInChildren<MeshFilter>();
            var skinnedMesh = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            if (mesh.Length != 0)
            {
                var bound = mesh[0].sharedMesh.bounds;
                headPosition = new Vector3(0, bound.max.y - bound.min.y, 0);
            }
            else if (skinnedMesh.Length != 0)
            {
                var bound = skinnedMesh[0].sharedMesh.bounds;
                headPosition = new Vector3(0, bound.max.y - bound.min.y, 0);
            }
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

