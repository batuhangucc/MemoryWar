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
    public GameObject walkStepFxPrefab;

    [Header("FX Ayarları")]
    public float fxDestroyTime = 1.2f;
    public float stepFxInterval = 0.18f;

    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer sr;

    float moveInput;
    float stepFxTimer;

    bool isGrounded;
    bool wasGrounded;

    // 🔒 TEK YÖN KAYNAĞI
    bool isFacingLeft = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        HandleFlip();

        // Ground check
        bool groundedNow = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        isGrounded = groundedNow;
        anim.SetBool("isGrounded", isGrounded);

        bool isRunning = Mathf.Abs(moveInput) > 0.01f;
        anim.SetBool("isRunning", isRunning);

        // Landing FX
        if (isGrounded && !wasGrounded)
            SpawnFx(landFxPrefab);

        wasGrounded = isGrounded;

        if (Input.GetButtonDown("Jump") && isGrounded)
            Jump();

        HandleStepFx(isRunning);

        anim.SetFloat("yVelocity", rb.linearVelocity.y);
    }

    void FixedUpdate()
    {
        float targetX = moveInput * moveSpeed;
        float currentX = rb.linearVelocity.x;
        float accel = Mathf.Abs(targetX) > 0.01f ? acceleration : deceleration;

        float newX = Mathf.MoveTowards(
            currentX,
            targetX,
            accel * Time.fixedDeltaTime
        );

        rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
    }

    // ---------------- FLIP (KESİN) ----------------

    void HandleFlip()
    {
        // 1️⃣ HAREKET VARSA → HER ZAMAN HAREKET YÖNÜ
        if (moveInput > 0.01f)
        {
            isFacingLeft = false;
        }
        else if (moveInput < -0.01f)
        {
            isFacingLeft = true;
        }
        // 2️⃣ SADECE HİÇ HAREKET YOKSA → mouse bakabilir
        else
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float mouseDir = mouseWorld.x - transform.position.x;

            if (Mathf.Abs(mouseDir) > 0.1f)
                isFacingLeft = mouseDir < 0f;
        }

        sr.flipX = isFacingLeft;
    }

    // ---------------- JUMP ----------------

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        anim.SetTrigger("jump");
        SpawnFx(jumpFxPrefab);
    }

    // ---------------- STEP FX ----------------

    void HandleStepFx(bool isRunning)
    {
        stepFxTimer -= Time.deltaTime;

        if (!isRunning || !isGrounded)
            return;

        if (Mathf.Abs(rb.linearVelocity.x) < 0.1f)
            return;

        if (stepFxTimer <= 0f)
        {
            SpawnStepFx();
            stepFxTimer = stepFxInterval;
        }
    }

    void SpawnStepFx()
    {
        if (walkStepFxPrefab == null) return;

        float xOffset = isFacingLeft ? 0.1f : -0.1f;

        Vector3 pos = new Vector3(
            groundCheck.position.x + xOffset,
            groundCheck.position.y,
            0f
        );

        GameObject fx = Instantiate(walkStepFxPrefab, pos, Quaternion.identity);
        Destroy(fx, fxDestroyTime);
    }

    // ---------------- FX SPAWN ----------------

    void SpawnFx(GameObject fxPrefab)
    {
        if (fxPrefab == null) return;

        GameObject fx = Instantiate(
            fxPrefab,
            groundCheck.position,
            Quaternion.identity
        );

        Destroy(fx, fxDestroyTime);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}