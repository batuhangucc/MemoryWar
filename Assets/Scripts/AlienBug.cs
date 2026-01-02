using UnityEngine;
using System.Collections;

// EnemyBase'den miras alıyor
public class AlienBug : EnemyBase 
{
    [Header("Hareket ve Devriye")]
    public float moveSpeed = 2f;
    public float patrolRadius = 3f;
    public float waitTime = 1f;

    [Header("Saldırı (Güdümlü Zıplama)")]
    public float maxJumpForceX = 8f;
    public float jumpForceY = 8f;
    public float aggroRange = 7f;
    public float jumpCooldown = 2f;
    public float attackWindUp = 0.4f;

    [Header("Zemin Kontrolü")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    
    // NOT: rb, anim, health, lootPrefab ARTIK YOK. HEPSİ EnemyBase'den GELİYOR.

    [Header("Efektler")]
    public GameObject deathFxPrefab; 

    private Transform player;
    private Vector2 startPos;
    private Vector2 patrolTarget;

    private float nextJumpTime;
    private float waitTimer;
    private bool isGrounded;
    private bool isAttacking = false;

    private enum State { Patrol, Chase }
    private State currentState;

    // Start override ediyoruz
    void Start()
    {
        // Player'ı bul
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        startPos = transform.position;
        SelectNewPatrolPoint();
    }

    void Update()
    {
        if (isDead) return; // EnemyBase'den gelen değişken
        if (isAttacking) return;

        // Ground Check
        if(groundCheck != null)
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // State Machine
        if (player != null && Vector2.Distance(transform.position, player.position) < aggroRange)
        {
            currentState = State.Chase;
        }
        else
        {
            currentState = State.Patrol;
        }

        switch (currentState)
        {
            case State.Patrol:
                PatrolLogic();
                break;
            case State.Chase:
                ChaseLogic();
                break;
        }
    }

    // --- LOGIC ---
    void PatrolLogic()
    {
        if (!isGrounded) return;

        transform.position = Vector2.MoveTowards(transform.position, patrolTarget, moveSpeed * Time.deltaTime);
        // EnemyBase'den gelen 'anim'
        FlipSprite(patrolTarget.x);

        if (Vector2.Distance(transform.position, patrolTarget) < 0.1f)
        {
            
            if (waitTimer <= 0) waitTimer = waitTime;
            else
            {
                waitTimer -= Time.deltaTime;
                if (waitTimer <= 0) SelectNewPatrolPoint();
            }
        }
    }

    void ChaseLogic()
    {
        FlipSprite(player.position.x);
        

        if (isGrounded && Time.time > nextJumpTime)
        {
            StartCoroutine(AttackRoutine());
        }
    }

   IEnumerator AttackRoutine()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero; // Dur ve odaklan
        anim.SetTrigger("Attack");
        
        // Hazırlık (Wind-up)
        yield return new WaitForSeconds(attackWindUp);

        if (player != null && !isDead)
        {
            // --- DÜZELTİLEN FİZİK KISMI ---
            
            // 1. Oyuncunun yönünü ve mesafesini al
            Vector2 direction = (player.position - transform.position).normalized;
            float distance = Vector2.Distance(transform.position, player.position);

            // 2. İLERİ KUVVETİ (X) ARTIRILDI
            // Eski çarpan 1.5 idi, bunu 4.0 yaptık. Böylece karakter ileriye daha sert atılır.
            // Minimum atılma gücünü de 3'ten 6'ya çıkardık ki kısa mesafede bile 'hop' diye kalmasın.
            float aggressiveXForce = Mathf.Clamp(distance * 4.0f, 6f, maxJumpForceX);
            
            // 3. YUKARI KUVVETİ (Y) DÜŞÜRÜLDÜ VE DENGELENDİ
            // Eğer oyuncu çok yakındaysa Y gücünü biraz kısıyoruz ki üzerinden uçup gitmesin.
            // distance < 2 ise gücü %60 kullan, değilse tam güç (jumpForceY) kullan.
            float adjustedYForce = (distance < 2f) ? jumpForceY * 0.6f : jumpForceY;

            // Vektörü oluştur: X (Yön * Güç), Y (Ayarlanmış Yükseklik)
            Vector2 jumpVector = new Vector2(direction.x * aggressiveXForce, adjustedYForce);
            
            rb.AddForce(jumpVector, ForceMode2D.Impulse);
        }

        nextJumpTime = Time.time + jumpCooldown;
        
        // Havada kalma süresi (Yere düşünce zaten PatrolLogic çalışmaya başlayacak)
        yield return new WaitForSeconds(0.5f); 
        isAttacking = false;
    }

    // Çarpışma Mantığı
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;
        if (collision.gameObject.CompareTag("Player"))
        {
            Player playerScript = collision.gameObject.GetComponent<Player>();
            if (playerScript != null)
            {
                playerScript.TakeDamage(10, transform.position);
                rb.linearVelocity = Vector2.zero;
                rb.AddForce(new Vector2(-Mathf.Sign(transform.localScale.x) * 3f, 3f), ForceMode2D.Impulse);
            }
        }
    }

    // --- OVERRIDE ---
    // Ölüm fonksiyonunu override ediyoruz çünkü özel efektimiz (deathFx) var
    protected override void Die()
    {
        // Efekti patlat
        if (deathFxPrefab) Instantiate(deathFxPrefab, transform.position, Quaternion.identity);

        // Geri kalan (Animasyon, Loot, Destroy) işini babaya bırak
        base.Die(); 
    }

    void SelectNewPatrolPoint()
    {
        float randomX = Random.Range(-patrolRadius, patrolRadius);
        patrolTarget = new Vector2(startPos.x + randomX, transform.position.y);
    }

    void FlipSprite(float targetX)
    {
        if (targetX > transform.position.x)
            transform.localScale = new Vector3(1, 1, 1);
        else
            transform.localScale = new Vector3(-1, 1, 1);
    }
}