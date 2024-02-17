using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Script.GamePlay
{
    [System.Serializable]
    public class SpawnPlace
    {
        [SerializeField] private List<Transform> _spotList = new List<Transform>();
        private Dictionary<string, Transform> _spotDictionary;

        public int Length => _spotList.Count;
        
        public void Initialize()
        {
            _spotDictionary = new Dictionary<string, Transform>();
            foreach (var spot in _spotList)
            {
                AddSpot(spot);
            }
        }

        public Transform AddSpot(Transform spot, string name = null)
        {
            if (name == null)
                _spotDictionary.Add(spot.name, spot);
            else 
                _spotDictionary.Add(name, spot);
            return spot;
        }

        public Transform GetSpot(string name) => _spotDictionary[name];
        public Transform GetSpot(int value) => _spotDictionary.Values.ToArray()[value];

        public Transform GetRandomSpot()
        {
            if (_spotDictionary.Count == 0)
                return null;
            
            var r = Random.Range(0, _spotDictionary.Count);
            return GetSpot(r);
        }

        public void SetAllSpotActive(bool value)
        {
            foreach (var (key, spotTransform) in _spotDictionary)
                spotTransform.gameObject.SetActive(value);
        }
    }
}