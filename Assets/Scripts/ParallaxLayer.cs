using UnityEngine;

// Fondo con efecto parallax
public class ParallaxLayer : MonoBehaviour
{
    public float factorX = 0.3f;
    public float yOffset = 0f;
    Transform cam;

    void Start() { if (Camera.main) cam = Camera.main.transform; }

    void LateUpdate()
    {
        if (!cam) return;
        transform.position = new Vector3(cam.position.x * factorX, cam.position.y + yOffset, 0f);
    }
}
