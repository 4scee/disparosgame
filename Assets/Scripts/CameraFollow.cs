using UnityEngine;

// 3.4 cámara fluida y proporcional
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 0.15f;
    public Vector3 offset = new Vector3(2f, 1.5f, -10f);
    public float orthoSize = 5.5f;
    public bool useLimits = true;
    public float minX = -5f, maxX = 45f, minY = -2f, maxY = 10f;
    Vector3 vel;

    void Start()
    {
        var cam = GetComponent<Camera>();
        if (cam) { cam.orthographic = true; cam.orthographicSize = orthoSize; }
        if (!target) { var p = GameObject.FindWithTag("Player"); if (p) target = p.transform; }
    }

    void LateUpdate()
    {
        if (!target) return;
        float side = target.localScale.x >= 0 ? 1 : -1;
        Vector3 desired = target.position + new Vector3(offset.x * side, offset.y, offset.z);
        if (useLimits)
        {
            desired.x = Mathf.Clamp(desired.x, minX, maxX);
            desired.y = Mathf.Clamp(desired.y, minY, maxY);
        }
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref vel, smoothTime);
    }
}
