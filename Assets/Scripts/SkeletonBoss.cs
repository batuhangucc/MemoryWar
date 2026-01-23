using UnityEngine;
using System.Collections;
using UnityEngine.UI; 

public class SkeletonBoss : EnemyBase 
{
    // --- (Diğer değişkenlerin aynen kalsın) ---
    [Header("UI Ayarları")]
    public GameObject bossHUDPanel; 
    public Image healthFillImage;   
    [Range(1f, 25f)] public float smoothSpeed = 10f; 

    [Header("Sprite ve Hareket")]
    public bool spriteSolaBakiyor = false; 
    public float moveSpeed = 2f;      
    public float attackRange = 6f;    
    public float aggroRange = 10f; 

    [Header("Saldırı Ayarları")]
    public GameObject projectilePrefab; 
    public Transform firePoint;         
    public float attackCooldown = 3f;
    public float attackAnimDuration = 1.5f; // Saldırı animasyon süren

    [Header("Hasar Ayarları")]
    public float damageAnimCooldown = 0.2f; 
    
    private Transform player;
    private float nextAttackTime;
    private bool isAttacking = false;
    private float defaultScaleX; 
    private bool isBattleStarted = false;
    private float nextDamageAnimTime;

    private float targetWidth;     
    private float fullWidth;       
    private RectTransform barRect; 

    void Start()
    {
        // ... (Start kodların aynen kalsın) ...
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        currentHealth = maxHealth;
        defaultScaleX = Mathf.Abs(transform.localScale.x);

        if (healthFillImage != null)
        {
            barRect = healthFillImage.rectTransform;
            fullWidth = barRect.sizeDelta.x;
            targetWidth = fullWidth;
        }
        if (bossHUDPanel != null) bossHUDPanel.SetActive(false); 
    }

    void Update()
    {
        // ... (Update kodların aynen kalsın) ...
        if (healthFillImage != null && barRect != null)
        {
            float currentWidth = barRect.sizeDelta.x;
            float newWidth = Mathf.Lerp(currentWidth, targetWidth, smoothSpeed * Time.deltaTime);
            barRect.sizeDelta = new Vector2(newWidth, barRect.sizeDelta.y);
        }

        if (isDead || player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (!isBattleStarted)
        {
            if (distanceToPlayer <= aggroRange) StartBattle();
            else { anim.SetBool("IsMoving", false); return; }
        }

        // Saldırıyorsa kımıldama
        if (isAttacking) 
        {
            rb.linearVelocity = Vector2.zero; 
            return;
        }

        if (distanceToPlayer > aggroRange * 1.5f) { anim.SetBool("IsMoving", false); return; }

        FacePlayer();

        if (distanceToPlayer > attackRange)
        {
            Vector2 targetPosition = new Vector2(player.position.x, transform.position.y);
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            anim.SetBool("IsMoving", true); 
        }
        else
        {
            rb.linearVelocity = Vector2.zero; 
            anim.SetBool("IsMoving", false); 

            if (Time.time > nextAttackTime)
            {
                StartCoroutine(AttackRoutine());
            }
        }
    }

    void StartBattle() { isBattleStarted = true; if (bossHUDPanel != null) bossHUDPanel.SetActive(true); }

    public override void TakeDamage(int damage)
    {
        if (!isBattleStarted) return; 
        base.TakeDamage(damage);

        // 🔥 GÜNCELLEME: Trigger'ı sadece saldırı yoksa tetikle
        // Ama asıl korumayı Animator'daki Condition (IsAttacking == false) yapacak!
        if (anim != null && !isDead && !isAttacking && Time.time >= nextDamageAnimTime)
        {
            anim.SetTrigger("Damage"); 
            nextDamageAnimTime = Time.time + damageAnimCooldown; 
        }

        if (healthFillImage != null)
        {
            targetWidth = ((float)currentHealth / maxHealth) * fullWidth;
        }
    }

    protected override void Die()
    {
        if (bossHUDPanel != null) bossHUDPanel.SetActive(false);
        base.Die();
    }

    // 🔥 KRİTİK KISIM BURASI 🔥
    IEnumerator AttackRoutine()
    {
        isAttacking = true; 
        anim.SetBool("IsAttacking", true); 
        
        anim.SetBool("IsMoving", false);
        
        
        // --- DEĞİŞEN KISIM: Süre saymak yerine animasyonu bekle ---
        
        // 1. Frame bekle (Animasyonun başlaması için)
        yield return new WaitForEndOfFrame(); 
        
        // 2. Şu anki animasyonun uzunluğunu otomatik al
        // (0. katman, "Skeleton_Attack" animasyonunun uzunluğu)
        float animationLength = anim.GetCurrentAnimatorStateInfo(0).length;
        
        // 3. O süre kadar bekle
        yield return new WaitForSeconds(animationLength); 
        
        // -----------------------------------------------------------

        anim.SetBool("IsAttacking", false);
        isAttacking = false; 
        
        nextAttackTime = Time.time + attackCooldown;
    }

    // ... (ReleaseSpell ve FacePlayer aynen kalsın) ...
    public void ReleaseSpell()
    {
        if (isDead) return;
        if (projectilePrefab != null && firePoint != null)
        {
            Quaternion bulletRotation = Quaternion.identity;
            if (transform.localScale.x < 0) { if (!spriteSolaBakiyor) bulletRotation = Quaternion.Euler(0, 0, 180); }
            else { if (spriteSolaBakiyor) bulletRotation = Quaternion.Euler(0, 0, 180); }
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