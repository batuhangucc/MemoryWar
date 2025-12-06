using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class Player : MonoBehaviour
{
    [Header("Hareket")]
    public float moveSpeed = 4f;
    public float acceleration = 12f;
    public float deceleration = 25f;

    [Header("Zıplama")]
    public float jumpForce = 7f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;

    [Header("FX")]
    public GameObject jumpFxPrefab;
    public GameObject landFxPrefab;

    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer sr;

    float moveInput;
    bool isGrounded;
    bool wasGrounded;

    void Awake()
    {
        rb   = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr   = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // --- Input ---
        moveInput = Input.GetAxisRaw("Horizontal");

        // --- Sprite yönü ---
        if (moveInput > 0.01f)
            sr.flipX = false;
        else if (moveInput < -0.01f)
            sr.flipX = true;

        // --- Ground Check ---
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        anim.SetBool("isGrounded", isGrounded);

        // --- Koşma animasyonu ---
        anim.SetBool("isRunning", Mathf.Abs(moveInput) > 0.01f);

        // --- Landing FX ---
        if (isGrounded && !wasGrounded)
            SpawnFx(landFxPrefab);

        wasGrounded = isGrounded;

        // --- Jump Input ---
        if (Input.GetButtonDown("Jump") && isGrounded)
            Jump();

        // --- Düşüş animasyonu için hız ---
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
    }

    void FixedUpdate()
    {
        // Hedef x hızı
        float targetX = moveInput * moveSpeed;
        float currentX = rb.linearVelocity.x;

        // acceleration / deceleration seçimi
        float accel = Mathf.Abs(moveInput) > 0.01f ? acceleration : deceleration;

        // daha yumuşak hızlandırma
        float newX = Mathf.MoveTowards(currentX, targetX, accel * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
    }

    void Jump()
    {
        // Daha kontrollü jump hissi
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);

        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        anim.SetTrigger("jump");

        SpawnFx(jumpFxPrefab);
    }

    void SpawnFx(GameObject fxPrefab)
{
    if (fxPrefab == null) return;

    GameObject fx = Instantiate(
        fxPrefab,
        new Vector3(groundCheck.position.x, groundCheck.position.y, 0f),
        Quaternion.identity
    );

    // 1 saniye sonra otomatik sil
    Destroy(fx, 1f);
}

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}