using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 1.6f;
    public float range = 10f;
    public int bulletCount = 1;
    public float spreadAngle = 12f;
    public bool aimAtPlayer;
    Transform player;
    float next;

    void Start()
    {
        var p = GameObject.FindWithTag("Player"); if (p) player = p.transform;
        next = Time.time + Random.Range(0.6f, fireRate);
    }

    void Update()
    {
        if (!player) return;
        float dx = player.position.x - transform.position.x;
        if (Mathf.Abs(dx) > range || Mathf.Abs(player.position.y - transform.position.y) > 4f) return;
        if (Time.time < next) return;
        float facing = transform.localScale.x >= 0 ? 1f : -1f;
        if (!aimAtPlayer && Mathf.Sign(dx) != facing) return;
        next = Time.time + fireRate;

        Vector2 baseDir = aimAtPlayer ? ((Vector2)(player.position - firePoint.position)).normalized : new Vector2(facing, 0f);
        for (int i = 0; i < bulletCount; i++)
        {
            float ang = (i - (bulletCount - 1) / 2f) * spreadAngle;
            Vector2 dir = (Vector2)(Quaternion.Euler(0, 0, ang) * (Vector3)baseDir);
            var b = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            b.GetComponent<EnemyBullet>().Init(dir);
        }
        Sfx.Play("shoot", 0.35f);
    }
}
