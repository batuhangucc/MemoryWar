using UnityEngine;
using System.Collections;
using UnityEngine.UI; 

public class SkeletonBoss : EnemyBase 
{
    [Header("UI Ayarları")]
    public GameObject bossHUDPanel; 
    public Image healthFillImage;   
    
    [Range(1f, 25f)] 
    public float smoothSpeed = 10f; 

    [Header("Sprite Yönü Ayarı")]
    public bool spriteSolaBakiyor = false; 

    [Header("Hareket Ayarları")]
    public float moveSpeed = 2f;      
    public float attackRange = 6f;    
    public float aggroRange = 10f; 

    [Header("Saldırı Ayarları")]
    public GameObject projectilePrefab; 
    public Transform firePoint;         
    public float attackCooldown = 3f;   
    
    private Transform player;
    private float nextAttackTime;
    private bool isAttacking = false;
    private float defaultScaleX; 
    private bool isBattleStarted = false;

    // Sliced bar için gerekli değişkenler
    private float targetWidth; // Gitmek istediğimiz genişlik
    private float fullWidth;   // Barın full halindeki genişliği (Başlangıçta ölçeceğiz)
    private RectTransform barRect; // Genişliği değiştirmek için gerekli bileşen

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        
        currentHealth = maxHealth;
        defaultScaleX = Mathf.Abs(transform.localScale.x);

        // --- SLICED BAR KURULUMU ---
        if (healthFillImage != null)
        {
            // Barın RectTransform bileşenini al
            barRect = healthFillImage.rectTransform;
            
            // Başlangıçtaki genişliği "Full Can" genişliği olarak kaydet
            // ÖNEMLİ: Unity'de barı sahnede tam dolu haliyle ayarlamış olmalısın!
            fullWidth = barRect.sizeDelta.x;
            
            // Hedefi full yap
            targetWidth = fullWidth;
        }

        if (bossHUDPanel != null)
        {
            bossHUDPanel.SetActive(false); 
        }
    }

    void Update()
    {
        // --- SMOOTH WIDTH (GENİŞLİK) SİSTEMİ ---
        if (healthFillImage != null && barRect != null)
        {
            // Mevcut genişliği, hedef genişliğe doğru kaydır
            float currentWidth = barRect.sizeDelta.x;
            float newWidth = Mathf.Lerp(currentWidth, targetWidth, smoothSpeed * Time.deltaTime);
            
            // Yeni genişliği uygula
            barRect.sizeDelta = new Vector2(newWidth, barRect.sizeDelta.y);
        }

        if (isDead || player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (!isBattleStarted)
        {
            if (distanceToPlayer <= aggroRange)
            {
                StartBattle();
            }
            else
            {
                anim.SetBool("IsMoving", false);
                return; 
            }
        }

        if (isAttacking) 
        {
            rb.linearVelocity = Vector2.zero; 
            return;
        }

        if (distanceToPlayer > aggroRange * 1.5f) 
        {
            anim.SetBool("IsMoving", false);
            return;
        }

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

    void StartBattle()
    {
        isBattleStarted = true; 
        if (bossHUDPanel != null) bossHUDPanel.SetActive(true); 
    }

    public override void TakeDamage(int damage)
    {
        if (!isBattleStarted) return; 

        base.TakeDamage(damage);

        // --- HEDEF GENİŞLİĞİ GÜNCELLE ---
        if (healthFillImage != null)
        {
            // Oran hesabı: (Mevcut Can / Max Can) * Full Genişlik
            float healthPercentage = (float)currentHealth / maxHealth;
            targetWidth = healthPercentage * fullWidth;
        }
    }

    protected override void Die()
    {
        if (bossHUDPanel != null) bossHUDPanel.SetActive(false);
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