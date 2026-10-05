using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 5;
    public float invulnerableTime = 1f;
    public float fallLimitY = -12f;
    public int Health { get; private set; }
    public int MaxHealth => maxHealth;
    float lastHit = -99f;
    bool dead;
    SpriteRenderer sr;

    void Awake() { Health = maxHealth; sr = GetComponent<SpriteRenderer>(); }

    void Update()
    {
        if (transform.position.y < fallLimitY) { Health = 0; Die(); }
        if (sr && !dead) sr.enabled = Time.time >= lastHit + invulnerableTime || ((int)(Time.time * 15f) % 2 == 0);
    }

    public void TakeDamage(int dmg)
    {
        if (dead || Time.time < lastHit + invulnerableTime) return;
        lastHit = Time.time;
        Health -= dmg;
        Sfx.Play("hurt");
        if (Health <= 0) Die();
    }

    public void Heal(int n) { Health = Mathf.Min(maxHealth, Health + n); }

    void Die()
    {
        if (dead) return;
        dead = true;
        if (sr) sr.enabled = true;
        GetComponent<PlayerController>().enabled = false;
        GetComponent<PlayerShooter>().enabled = false;
        var rb = GetComponent<Rigidbody2D>(); rb.linearVelocity = Vector2.zero;
        Sfx.Play("explosion");
        Invoke(nameof(Over), 0.9f);
    }

    void Over() { if (GameUI.Instance) GameUI.Instance.ShowGameOver(); }
}
