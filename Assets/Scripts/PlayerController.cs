using UnityEngine;

// 3.3 gravedad/salto/colisión · 3.5 movimiento · 3.6 animaciones conectadas
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 6f;
    public float jumpForce = 13f;
    public float gravityScale = 3f;
    public Transform groundCheck;
    public float groundRadius = 0.2f;
    public LayerMask groundLayer;

    Rigidbody2D rb;
    Animator anim;
    float inputX;
    bool grounded;

    public bool FacingRight => transform.localScale.x > 0;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        rb.gravityScale = gravityScale;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;
        inputX = Input.GetAxisRaw("Horizontal"); // flechas o A/D
        grounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);

        if (Input.GetButtonDown("Jump") && grounded)          // Espacio
            { rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce); Sfx.Play("jump", 0.4f); }

        // Voltear personaje (el FirePoint hijo se voltea con él)
        if (inputX > 0.01f) SetFacing(1);
        else if (inputX < -0.01f) SetFacing(-1);

        if (anim != null && anim.runtimeAnimatorController != null)
        {
            anim.SetFloat("Speed", Mathf.Abs(inputX));
            anim.SetBool("Grounded", grounded);
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(inputX * moveSpeed, rb.linearVelocity.y);
    }

    void SetFacing(int dir)
    {
        var s = transform.localScale;
        s.x = Mathf.Abs(s.x) * dir;
        transform.localScale = s;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck) Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
    }
}
