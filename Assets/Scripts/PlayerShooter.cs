using UnityEngine;

// 3.2 disparo desde FirePoint · 3.6 sin desfases · 3.7 cadencia uniforme
public class PlayerShooter : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;          // punto de disparo alineado al arma
    public float fireRate = 0.25f;       // segundos entre balas (cadencia constante)
    float nextFire;
    PlayerController pc;
    Animator anim;

    void Awake() { pc = GetComponent<PlayerController>(); anim = GetComponent<Animator>(); }

    void Update()
    {
        if (Time.timeScale == 0f) return;
        bool fire = Input.GetButton("Fire1") || Input.GetKey(KeyCode.J) || Input.GetKey(KeyCode.K);
        if (fire && Time.time >= nextFire)
        {
            nextFire = Time.time + fireRate;
            Shoot();
        }
    }

    void Shoot()
    {
        if (anim != null && anim.runtimeAnimatorController != null) anim.SetTrigger("Shoot");
        Sfx.Play("shoot", 0.4f);
        var go = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        go.GetComponent<Bullet>().Init(pc.FacingRight ? 1 : -1);
    }
}
