using UnityEngine;
using System.Collections;

public class SkeletonBoss : EnemyBase 
{
    [Header("Sprite Yönü Ayarı")]
    public bool spriteSolaBakiyor = false; 

    [Header("Boss Saldırı")]
    public GameObject projectilePrefab; 
    public Transform firePoint;         
    public float attackCooldown = 3f;   
    
    [Header("Boss Görüş")]
    public float aggroRange = 10f;      
    
    private Transform player;
    private float nextAttackTime;
    private bool isAttacking = false;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (isDead || player == null) return;
        if (isAttacking) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer > aggroRange) return;

        FacePlayer();

        if (Time.time > nextAttackTime)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true; 
        anim.SetTrigger("Attack");
        // Animasyon süresi (tahmini)
        yield return new WaitForSeconds(1f); 
        nextAttackTime = Time.time + attackCooldown;
        isAttacking = false; 
    }

    // 🔥 DÜZELTME BURADA 🔥
    public void ReleaseSpell()
    {
        if (isDead) return;

        if (projectilePrefab != null && firePoint != null)
        {
            // Boss şu an fiziksel olarak sola mı bakıyor? (Scale kontrolü)
            // Scale negatifse (-1) sola bakıyor demektir (veya tam tersi, ayara göre değişir)
            // En garantisi: firePoint'in "right" vektörüne bakmak yerine,
            // Boss'un scale'i negatifse mermiyi 180 derece döndürelim.

            Quaternion bulletRotation = Quaternion.identity; // Varsayılan: Sağa (0 derece)

            // Eğer Scale X negatifse, yön ters dönmüştür.
            if (transform.localScale.x < 0)
            {
                // Eğer "SpriteSolaBakiyor" seçili değilse ve scale -1 ise -> SOLA BAKIYORDUR.
                // Eğer "SpriteSolaBakiyor" seçiliyse ve scale -1 ise -> SAĞA BAKIYORDUR.
                // Bu karmaşayı önlemek için basit mantık: 
                // Scale işareti ile yönü belirle.
                
                // Normalde sağa bakan bir sprite -1 scale alınca sola bakar.
                // Bu durumda mermiyi 180 derece (Sola) çevir.
                 if (!spriteSolaBakiyor) bulletRotation = Quaternion.Euler(0, 0, 180);
            }
            else
            {
                 // Scale pozitif (+1).
                 // Eğer sprite orijinalde sola bakıyorsa, mermi de sola (180) gitmeli.
                 if (spriteSolaBakiyor) bulletRotation = Quaternion.Euler(0, 0, 180);
            }

            // Mermiyi hesaplanan rotasyonla oluştur
            Instantiate(projectilePrefab, firePoint.position, bulletRotation);
        }
    }

    void FacePlayer()
    {
        float direction = player.position.x - transform.position.x;
        if (Mathf.Abs(direction) < 0.1f) return;

        float currentScaleSize = Mathf.Abs(transform.localScale.x);

        if (direction > 0) // Oyuncu SAĞDA
        {
            float targetX = spriteSolaBakiyor ? -currentScaleSize : currentScaleSize;
            transform.localScale = new Vector3(targetX, transform.localScale.y, transform.localScale.z);
        }
        else // Oyuncu SOLDA
        {
            float targetX = spriteSolaBakiyor ? currentScaleSize : -currentScaleSize;
            transform.localScale = new Vector3(targetX, transform.localScale.y, transform.localScale.z);
        }
    }
}