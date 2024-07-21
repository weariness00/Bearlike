using UnityEngine;
using UnityEngine.UI;

public class MoveTexture : MonoBehaviour
{
    public Sprite[] textures;
    [SerializeField] private Image image;
    public float frameRate = 0.1f; // 프레임 전환 속도

    private int currentFrame;
    private float timer;
    
    void Start()
    {
        image = gameObject.GetComponent<Image>();
        currentFrame = 0;
        timer = 0f;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= frameRate)
        {
            timer -= frameRate;
            currentFrame = (currentFrame + 1) % textures.Length;
            image.sprite = textures[currentFrame];
        }
    }
}
