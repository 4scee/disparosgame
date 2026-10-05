using UnityEngine;

// 3.2 / 3.7 trayectoria lineal y constante, sin gravedad
[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    public float speed = 16f;
    public int damage = 1;
    public float lifeTime = 2f;

    public void Init(int dir)
    {
        var rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(dir * speed, 0f);
        var s = transform.localScale; s.x = Mathf.Abs(s.x) * dir; transform.localScale = s;
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.isTrigger || other.GetComponent<PlayerHealth>()) return;
        var enemy = other.GetComponent<Enemy>();
        if (enemy) enemy.TakeDamage(damage);
        Destroy(gameObject);
    }
}
