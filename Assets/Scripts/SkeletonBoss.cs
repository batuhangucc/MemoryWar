using UnityEngine;
using System.Collections;
using UnityEngine.UI; 

public class SkeletonBoss : EnemyBase 
{
    [Header("UI Ayarları")]
    public Slider healthBar; 

    [Header("Sprite Yönü Ayarı")]
    public bool spriteSolaBakiyor = false; 

    [Header("Hareket Ayarları")]
    public float moveSpeed = 2f;      
    public float attackRange = 6f;    
    public float aggroRange = 10f; // Savaşın başlama mesafesi

    [Header("Saldırı Ayarları")]
    public GameObject projectilePrefab; 
    public Transform firePoint;         
    public float attackCooldown = 3f;   
    
    private Transform player;
    private float nextAttackTime;
    private bool isAttacking = false;
    private float defaultScaleX; 

    // 🔥 YENİ DEĞİŞKEN: Savaş başladı mı?
    private bool isBattleStarted = false;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        
        currentHealth = maxHealth;
        defaultScaleX = Mathf.Abs(transform.localScale.x);

        // --- BAŞLANGIÇTA CAN BARINI GİZLE ---
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
            healthBar.gameObject.SetActive(false); // <--- GİZLENDİ
        }
    }

    void Update()
    {
        if (isDead || player == null) return;
        
        // Mesafe ölçümü
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // --- 1. DURUM: SAVAŞ HENÜZ BAŞLAMADIYSA ---
        if (!isBattleStarted)
        {
            // Oyuncu menzile girdi mi?
            if (distanceToPlayer <= aggroRange)
            {
                // 🔥 SAVAŞI BAŞLAT! 🔥
                StartBattle();
            }
            else
            {
                // Menzile girmediyse HİÇBİR ŞEY YAPMA (Donmuş gibi bekle)
                anim.SetBool("IsMoving", false);
                return; 
            }
        }

        // --- SAVAŞ BAŞLADIKTAN SONRAKİ NORMAL DAVRANIŞLAR ---

        if (isAttacking) 
        {
            rb.linearVelocity = Vector2.zero; 
            return;
        }

        // Eğer savaş başladıysa ama oyuncu çok uzaklaştıysa takibi bırakabilir 
        // (İstersen burayı kaldırıp sonsuza kadar kovalatabilirsin)
        if (distanceToPlayer > aggroRange * 1.5f) // Çıkış menzili girişten biraz büyük olsun
        {
            anim.SetBool("IsMoving", false);
            return;
        }

        FacePlayer();

        if (distanceToPlayer > attackRange)
        {
            // Yürü
            Vector2 targetPosition = new Vector2(player.position.x, transform.position.y);
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            anim.SetBool("IsMoving", true); 
        }
        else
        {
            // Dur ve Saldır
            rb.linearVelocity = Vector2.zero; 
            anim.SetBool("IsMoving", false); 

            if (Time.time > nextAttackTime)
            {
                StartCoroutine(AttackRoutine());
            }
        }
    }

    // --- SAVAŞI BAŞLATAN FONKSİYON ---
    void StartBattle()
    {
        isBattleStarted = true; // Artık hasar alabilir ve hareket edebilir
        
        if (healthBar != null)
        {
            healthBar.gameObject.SetActive(true); // <--- CAN BARI GELDİ
        }

        // İstersen burada Boss bir kükreme sesi çıkarabilir veya animasyon yapabilir.
    }

    // --- HASAR ALMA (GÜNCELLENDİ) ---
    public override void TakeDamage(int damage)
    {
        // 🔥 ÖNEMLİ: Savaş başlamadıysa hasarı reddet!
        if (!isBattleStarted) return; 

        base.TakeDamage(damage);

        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }
    }

    protected override void Die()
    {
        if (healthBar != null)
        {
            healthBar.gameObject.SetActive(false);
        }
        base.Die();
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true; 
        anim.SetBool("IsMoving", false);
        anim.SetTrigger("Attack");
        yield return new WaitForSeconds(1f); 
        nextAttackTime = Time.time + attackCooldown;
        isAttacking = false; 
    }

    public void ReleaseSpell()
    {
        if (isDead) return;

        if (projectilePrefab != null && firePoint != null)
        {
            Quaternion bulletRotation = Quaternion.identity;
            if (transform.localScale.x < 0)
            {
                 if (!spriteSolaBakiyor) bulletRotation = Quaternion.Euler(0, 0, 180);
            }
            else
            {
                 if (spriteSolaBakiyor) bulletRotation = Quaternion.Euler(0, 0, 180);
            }
            Instantiate(projectilePrefab, firePoint.position, bulletRotation);
        }
    }

    void FacePlayer()
    {
        float direction = player.position.x - transform.position.x;
        if (Mathf.Abs(direction) < 0.1f) return;

        if (direction > 0) 
        {
            float targetX = spriteSolaBakiyor ? -defaultScaleX : defaultScaleX;
            transform.localScale = new Vector3(targetX, transform.localScale.y, transform.localScale.z);
        }
        else 
        {
            float targetX = spriteSolaBakiyor ? defaultScaleX : -defaultScaleX;
            transform.localScale = new Vector3(targetX, transform.localScale.y, transform.localScale.z);
        }
    }
}