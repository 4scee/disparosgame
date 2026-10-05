using UnityEngine;

// Animación por frames para monedas, enemigos, explosiones, etc.
public class SpriteAnimator : MonoBehaviour
{
    public Sprite[] frames;
    public float fps = 8f;
    public bool loop = true;
    public bool destroyAtEnd;
    SpriteRenderer sr;
    float t;

    void Awake() { sr = GetComponent<SpriteRenderer>(); }

    void Update()
    {
        if (frames == null || frames.Length == 0) return;
        t += Time.deltaTime * fps;
        int i = (int)t;
        if (i >= frames.Length)
        {
            if (destroyAtEnd) { Destroy(gameObject); return; }
            if (loop) { t %= frames.Length; i = (int)t; } else i = frames.Length - 1;
        }
        sr.sprite = frames[i];
    }
}
