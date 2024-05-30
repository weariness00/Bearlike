using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class TestVFXWeapon : MonoBehaviour
{
    public Animator animator;
    public VisualEffect smoke;
    public Material shotMat;
    
    void Start()
    {
        smoke.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            animator.SetTrigger("tFire");
            
            if(false == smoke.gameObject.activeSelf)
                smoke.gameObject.SetActive(true);
            smoke.SendEvent("OnPlay");
            StartCoroutine(OverHeatCoroutine());
        }
    }

    // weaponSystem에서 작동해여 코루틴이 끝까지 작동함
    IEnumerator OverHeatCoroutine()
    {
        float value;
        
        float elapsedTime = 0f;
        float duration = 0.6f; // 보간에 걸리는 시간
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            value = Mathf.Lerp(0, 0.8f, elapsedTime / duration);
            shotMat.SetFloat("_Value", value);
            Debug.Log($"value : {value}");
            yield return null;
        }
        
        while (elapsedTime > 0.0f)
        {
            elapsedTime -= Time.deltaTime;
            value = Mathf.Lerp(0, 0.8f, elapsedTime / duration);
            shotMat.SetFloat("_Value", value);
            Debug.Log($"value : {value}");
            yield return null;
        }
        shotMat.SetFloat("_Value", 0.0f);
    }
}
