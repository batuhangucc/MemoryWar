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
    public GameObject jumpFxPrefab;       // zıplarken çıkan toz
    public GameObject landFxPrefab;       // yere inerken çıkan toz
    public GameObject walkStepFxPrefab;   // yürürken çıkan adım tozu

    // Adım FX ayarları
    public float stepFxInterval = 0.18f;  // adımlar arası süre
    float stepFxTimer = 0f;

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
        if (moveInput > 0.01f)      sr.flipX = false;
        else if (moveInput < -0.01f) sr.flipX = true;

        // --- Ground check ---
        bool groundedNow = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        isGrounded = groundedNow;
        anim.SetBool("isGrounded", isGrounded);

        // --- Koşma animasyonu ---
        bool isRunning = Mathf.Abs(moveInput) > 0.01f;
        anim.SetBool("isRunning", isRunning);

        // --- Landing FX (havadan yere iniş anı) ---
        if (isGrounded && !wasGrounded)
        {
            SpawnFx(landFxPrefab);
        }
        wasGrounded = isGrounded;

        // --- Zıplama ---
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }

        // --- Yürürken adım FX ---
        HandleStepFx(isRunning);

        // Animator’a düşüş hızını yollayalım (Fall state için)
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
    }

    void FixedUpdate()
    {
        // Hedef X hız
        float targetX = moveInput * moveSpeed;

        // Şu anki X hız
        float currentX = rb.linearVelocity.x;

        // Hareketteysek acceleration, dururken deceleration
        float accel = (Mathf.Abs(targetX) > 0.01f) ? acceleration : deceleration;

        // Hızı yumuşakça hedefe yaklaştır
        float newX = Mathf.MoveTowards(currentX, targetX, accel * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
    }

    void Jump()
    {
        // Eski düşüş hızını temizle
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);

        // Yukarı doğru impuls
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        // Jump animasyonu
        anim.SetTrigger("jump");

        // Jump FX
        SpawnFx(jumpFxPrefab);
    }

    // ---------------- FX Fonksiyonları ----------------

    void HandleStepFx(bool isRunning)
    {
        // Sayaç güncelle
        stepFxTimer -= Time.deltaTime;

        // Yürümüyor veya havadaysa FX spawnlama
        if (!isRunning || !isGrounded)
            return;

        // Çok yavaş kayarken gereksiz FX çıkmasın
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

        // Küçük bir pozisyon offset’i (karakterin ayağının biraz arkası)
        float xOffset = sr.flipX ? 0.1f : -0.1f;

        Vector3 pos = new Vector3(
            groundCheck.position.x + xOffset,
            groundCheck.position.y,
            0f
        );

        Instantiate(walkStepFxPrefab, pos, Quaternion.identity);
    }

    void SpawnFx(GameObject fxPrefab)
    {
        if (fxPrefab == null) return;

        Instantiate(
            fxPrefab,
            new Vector3(groundCheck.position.x, groundCheck.position.y, 0f),
            Quaternion.identity
        );
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}