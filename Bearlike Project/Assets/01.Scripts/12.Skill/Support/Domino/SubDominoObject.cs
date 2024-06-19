using System;
using System.Collections;
using UnityEngine;

namespace Skill.Support
{
    public class SubDominoObject : MonoBehaviour
    {
        [SerializeField] private bool isEnable = false; 
        
        private void OnCollisionEnter(Collision other)
        {
            if (isEnable == false) return;
            if (other.gameObject.TryGetComponent(out SubDominoObject sdObj))
            {
                Destroy(gameObject);
                sdObj.OnEnableCoroutine();
            }
        }

        private void OnEnableCoroutine() => StartCoroutine(EnableCoroutine());

        IEnumerator EnableCoroutine()
        {
            yield return new WaitForEndOfFrame();
            isEnable = true;
        }
    }
}