using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyBullet : MonoBehaviour
{
    public float speed = 7f;
    public int damage = 1;
    public float lifeTime = 4f;

    public void Init(Vector2 dir)
    {
        var rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearVelocity = dir.normalized * speed;
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D o)
    {
        if (o.isTrigger) return;
        var p = o.GetComponent<PlayerHealth>();
        if (p) { p.TakeDamage(damage); Destroy(gameObject); return; }
        if (o.GetComponent<Enemy>()) return;
        Destroy(gameObject);
    }
}
