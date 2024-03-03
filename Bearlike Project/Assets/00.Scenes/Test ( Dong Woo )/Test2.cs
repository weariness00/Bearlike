using System;
using System.Collections.Generic;
using UnityEngine;

namespace _00.Scenes.Test___Dong_Woo__
{
    public class Test2 : MonoBehaviour
    {
        private void Start()
        {
            
        }

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.CompareTag("Destruction"))
            {
                Debug.LogError("asdasdasd");
            }
        }
    }
}