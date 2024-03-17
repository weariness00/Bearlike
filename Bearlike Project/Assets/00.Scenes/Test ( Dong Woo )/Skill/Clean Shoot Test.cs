using UnityEngine;

public class CleanShootTest : MonoBehaviour
{
    public float muS = 10f;

    public bool isDestroy;
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var t = Camera.main.transform;
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.transform.position = t.position + t.forward * muS;
            box.transform.rotation = t.rotation;
            box.transform.localScale *= muS / 2;
            if(isDestroy) Destroy(box.gameObject, 5f);
            
        }
    }
}
