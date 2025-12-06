using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class Player : MonoBehaviour
{
    [Header("Hareket")]
    public float moveSpeed = 4f;
    public float acceleration = 10f;
    public float deceleration = 30f;

    [Header("Zıplama")]
    public float jumpForce = 7f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;

    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer sr;

    float moveInput;
    float targetSpeed;
    bool isGrounded;

    void Awake()
    {
        rb  = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr  = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Yatay input
        moveInput = Input.GetAxisRaw("Horizontal");

        // Sprite flip
        if (moveInput > 0.01f)      sr.flipX = false;
        else if (moveInput < -0.01f) sr.flipX = true;

        // Koşma animasyonu
        anim.SetBool("isRunning", Mathf.Abs(moveInput) > 0.01f);

        // Ground check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position,
                                             groundCheckRadius,
                                             groundLayer);
        anim.SetBool("isGrounded", isGrounded);

        // Y ekseni hızı (Jump ↔ Fall ayrımı için)
        anim.SetFloat("yVelocity", rb.linearVelocity.y);

        // Zıplama inputu
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }
    }

    void FixedUpdate()
    {
        // Hedef hız
        targetSpeed = moveInput * moveSpeed;

        // Şu anki hızla hedef arasındaki fark
        float speedDif = targetSpeed - rb.linearVelocity.x;
        float accelRate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;

        // X eksenine kuvvet uygula
        rb.AddForce(new Vector2(speedDif * accelRate, 0f));
    }

    void Jump()
    {
        // Eski dikey hızı sıfırla (daha net zıplama)
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);

        // Yukarı doğru impuls
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        // Jump trigger
        anim.SetTrigger("jump");
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}