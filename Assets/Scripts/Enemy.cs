using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    public int health = 3;
    public float speed = 2f;
    public float patrolDistance = 3f;
    public float sightRange = 9f;
    public float stopDistance = 4f;
    public int contactDamage = 1;
    public bool isBoss;
    public GameObject[] drops;
    public float dropChance = 0.6f;
    public GameObject deathEffect;

    public int Health => health;
    public int MaxHealth { get; private set; }

    Rigidbody2D rb;
    SpriteRenderer sr;
    Color baseColor = Color.white;
    float startX;
    int dir = -1;
    Transform player;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>(); rb.freezeRotation = true;
        sr = GetComponent<SpriteRenderer>(); if (sr) baseColor = sr.color;
        startX = transform.position.x; MaxHealth = health;
    }

    void Start() { var p = GameObject.FindWithTag("Player"); if (p) player = p.transform; }

    void FixedUpdate()
    {
        if (transform.position.y < -15f) { Destroy(gameObject); return; }
        float move;
        if (player && Mathf.Abs(player.position.x - transform.position.x) < sightRange && Mathf.Abs(player.position.y - transform.position.y) < 4f)
        {
            float dx = player.position.x - transform.position.x;
            dir = dx >= 0 ? 1 : -1;
            move = Mathf.Abs(dx) > stopDistance ? dir * speed : 0f;
        }
        else
        {
            if (transform.position.x < startX - patrolDistance) dir = 1;
            else if (transform.position.x > startX + patrolDistance) dir = -1;
            move = dir * speed;
        }
        rb.linearVelocity = new Vector2(move, rb.linearVelocity.y);
        var s = transform.localScale; s.x = Mathf.Abs(s.x) * dir; transform.localScale = s;
    }

    public void TakeDamage(int dmg)
    {
        health -= dmg;
        Sfx.Play("hit", 0.4f);
        if (sr) { sr.color = new Color(1f, 0.4f, 0.4f); CancelInvoke(nameof(ResetColor)); Invoke(nameof(ResetColor), 0.06f); }
        if (health <= 0) Die();
    }

    void ResetColor() { if (sr) sr.color = baseColor; }

    void Die()
    {
        Sfx.Play("explosion", 0.7f);
        if (deathEffect) Instantiate(deathEffect, transform.position, Quaternion.identity);
        if (drops != null && drops.Length > 0 && Random.value < dropChance)
            Instantiate(drops[Random.Range(0, drops.Length)], transform.position + Vector3.up * 0.3f, Quaternion.identity);
        if (isBoss)
        {
            GameManager.Instance.CompleteLevel(2);
            if (GameUI.Instance) GameUI.Instance.ShowVictory();
        }
        Destroy(gameObject);
    }

    void OnCollisionStay2D(Collision2D c)
    {
        var p = c.collider.GetComponent<PlayerHealth>();
        if (p) p.TakeDamage(contactDamage);
    }
}
