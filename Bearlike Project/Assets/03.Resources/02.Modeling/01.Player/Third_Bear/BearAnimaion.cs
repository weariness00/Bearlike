using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BearAnimation : MonoBehaviour
{
    /// <summary>
    /// 무슨 블랜드
    /// </summary>
    public enum MoveMotion
    {
        Idle = 0,
        Walk = 1,
        Right,
        Left,
        Back
    }
}
