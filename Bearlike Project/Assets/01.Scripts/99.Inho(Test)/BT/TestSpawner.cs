using Monster;
using UnityEngine;

public class TestSpawner : MonoBehaviour
{
    public GameObject prefab;

    public int count;
    public int height;
    
    void Start()
    {
        for (var i = 0; i < count; i++)
        {
            Instantiate(prefab, new Vector3(i * 10, height, 0), new Quaternion(0, 0, 0, 0));
        }
    }
}
