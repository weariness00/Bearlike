using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class FlippingCoinEffect : MonoBehaviour
{
    public Rigidbody rb;

    [Header("힘의 크기")] 
    public float forcePower = 2.0f;
    public float torqueMagnitude = 2.0f;
    
    [Header("힘의 방향")]
    public float forceX = 0;
    public float forceY = 1;
    public float forceZ = 0;
    
    void Start()
    {
        FlickCoin();
        // rb = GetComponent<Rigidbody>();
    }

    public void FlickCoin()
    {
        transform.localPosition = new Vector3(0, 0, 2);
        transform.localEulerAngles = new Vector3(0, 90, 0);
        
        Vector3 forceDirection = new Vector3(forceX, forceY, forceZ);  // 힘의 방향

        rb.AddForce(forceDirection * forcePower, ForceMode.Impulse);
        // 회전의 축을 캐릭터의 회전값에 right를 더해서 진행해야한다.
        rb.AddTorque(Vector3.right * torqueMagnitude , ForceMode.Impulse);
    }
}
