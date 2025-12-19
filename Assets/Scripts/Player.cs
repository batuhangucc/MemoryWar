using UnityEngine;
using System.Collections; // Coroutine için gerekli

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class Player : MonoBehaviour
{
    [Header("--- SAĞLIK (HEALTH) ---")]
    public int maxHealth = 100;
    [SerializeField] private int currentHealth; // Inspector'da görmek için Serialize
    public bool isDead = false;

    [Header("--- HASAR EFEKTİ (KNOCKBACK) ---")]
    public float knockbackForce = 5f; // Hasar alınca ne kadar savrulacak
    public float knockbackDuration = 0.2f; // Ne kadar süre kontrolden çıkacak
    public Color damageColor = Color.red; // Yanıp sönme rengi
    private bool isKnockedBack = false;

    [Header("--- HAREKET ---")]
    public float moveSpeed = 4f;
    public float acceleration = 12f;
    public float deceleration = 25f;

    [Header("--- ZIPLAMA ---")]
    public float jumpForce = 7f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;

    [Header("--- FX ---")]
    public GameObject jumpFxPrefab;
    public GameObject landFxPrefab;
    public GameObject walkStepFxPrefab;
    public float fxDestroyTime = 1.2f;
    public float stepFxInterval = 0.18f;

    Rigidbody2D rb;
    Animator anim;
    Camera cam;
    SpriteRenderer sr; // Renk değişimi için gerekli

    float moveInput;
    float stepFxTimer;

    [HideInInspector] public bool isGrounded;
    bool wasGrounded;
    [HideInInspector] public bool isFacingLeft = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>(); // SpriteRenderer'ı alıyoruz
        cam = Camera.main;
        
        // Canı fulle
        currentHealth = maxHealth;
    }

    void Update()
    {
        // Öldüyse hiçbir şey yapma
        if (isDead) return;

        // Knockback yiyorsa hareket edemesin
        if (isKnockedBack) return;

        moveInput = Input.GetAxisRaw("Horizontal");

        // Yön ve Flip işlemleri
        HandleFlip();

        // Ground check
        bool groundedNow = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        isGrounded = groundedNow;
        anim.SetBool("isGrounded", isGrounded);

        bool isRunning = Mathf.Abs(moveInput) > 0.01f;
        anim.SetBool("isRunning", isRunning);

        // Landing FX
        if (isGrounded && !wasGrounded) SpawnFx(landFxPrefab);
        wasGrounded = isGrounded;

        // Zıplama
        if (Input.GetButtonDown("Jump") && isGrounded) Jump();

        HandleStepFx(isRunning);
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
    }

    void FixedUpdate()
    {
        if (isDead || isKnockedBack) return; // Hasar yiyorsa fizik kontrolünü bizden al

        float targetX = moveInput * moveSpeed;
        float currentX = rb.linearVelocity.x;
        float accel = Mathf.Abs(targetX) > 0.01f ? acceleration : deceleration;

        float newX = Mathf.MoveTowards(currentX, targetX, accel * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
    }

    // ---------------- HASAR ALMA SİSTEMİ (YENİ) ----------------

    // Enemy Trigger ile çarpışırsa
    void OnTriggerEnter2D(Collider2D collision)
    {
        // Çarptığımız şeyin etiketi "Enemy" mi?
        if (collision.CompareTag("Enemy"))
        {
            // Düşmanın pozisyonunu alarak ters yöne savrulmayı hesapla
            TakeDamage(20, collision.transform.position); 
        }
    }

    // Enemy Fiziksel Collider ile çarpışırsa (Yedek)
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            TakeDamage(20, collision.transform.position);
        }
    }

    public void TakeDamage(int damage, Vector3 enemyPosition)
    {
        if (isDead) return;

        currentHealth -= damage;
        
        // Animator'daki "Damage" triggerını tetikle
        anim.SetTrigger("Damage");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Ölmediyse geri tepme (Knockback) uygula
            StartCoroutine(KnockbackRoutine(enemyPosition));
        }
    }

    IEnumerator KnockbackRoutine(Vector3 enemyPos)
    {
        isKnockedBack = true;
        rb.linearVelocity = Vector2.zero; // Mevcut hızı sıfırla

        // Düşman sağdaysa sola, soldaysa sağa fırlat
        Vector2 direction = (transform.position - enemyPos).normalized;
        // Hafif yukarı doğru da fırlat ki yere sürtmesin
        Vector2 force = new Vector2(direction.x * 0.5f, 0.5f) * knockbackForce; 
        
        rb.AddForce(force, ForceMode2D.Impulse);

        // Kırmızı yanıp sönme efekti
        Color originalColor = sr.color;
        sr.color = damageColor;

        yield return new WaitForSeconds(knockbackDuration);

        sr.color = originalColor; // Rengi düzelt
        isKnockedBack = false; // Kontrolü geri ver
    }

    void Die()
    {
        isDead = true;
        anim.SetTrigger("Die"); // Eğer Die animasyonun varsa
        rb.linearVelocity = Vector2.zero;
        // Çarpışmaları kapat ki düşmanlar içinden geçsin
        GetComponent<Collider2D>().enabled = false; 
        rb.gravityScale = 0; // İstersen havada asılı kalsın veya düşsün
        Debug.Log("Player Öldü!");
    }

    // ---------------- MEVCUT FONKSİYONLAR ----------------

    void HandleFlip()
    {
        bool isShooting = Input.GetMouseButton(0);

        if (isShooting) FaceMouse();
        else if (Mathf.Abs(moveInput) > 0.01f)
        {
            if (moveInput > 0) TurnRight();      
            else if (moveInput < 0) TurnLeft(); 
        }
        else FaceMouse();
    }

    void FaceMouse()
    {
        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        if (mouseWorld.x < transform.position.x) TurnLeft();
        else TurnRight();
    }

    void TurnLeft()
    {
        isFacingLeft = true;
        Vector3 scale = transform.localScale;
        scale.x = -1f * Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    void TurnRight()
    {
        isFacingLeft = false;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        anim.SetTrigger("jump");
        SpawnFx(jumpFxPrefab);
    }

    void HandleStepFx(bool isRunning)
    {
        stepFxTimer -= Time.deltaTime;
        if (isRunning && isGrounded && Mathf.Abs(rb.linearVelocity.x) > 0.1f && stepFxTimer <= 0f)
        {
            SpawnStepFx();
            stepFxTimer = stepFxInterval;
        }
    }

    void SpawnStepFx()
    {
        if (walkStepFxPrefab == null) return;
        float xOffset = isFacingLeft ? 0.2f : -0.2f;
        Vector3 pos = new Vector3(groundCheck.position.x + xOffset, groundCheck.position.y, 0f);
        GameObject fx = Instantiate(walkStepFxPrefab, pos, Quaternion.identity);
        Destroy(fx, fxDestroyTime);
    }

    void SpawnFx(GameObject fxPrefab)
    {
        if (fxPrefab) Destroy(Instantiate(fxPrefab, groundCheck.position, Quaternion.identity), fxDestroyTime);
    }
}