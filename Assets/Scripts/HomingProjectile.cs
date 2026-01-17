using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class HomingProjectile : MonoBehaviour
{
    [Header("Mermi Ayarları")]
    public float speed = 6f;        
    public float rotateSpeed = 200f; 
    public int damage = 20;         
    public float lifetime = 5f;     

    [Header("Güdüm Ayarı")]
    public float homingDelay = 0.5f;

    [Header("Görsel Ayarı")]
    // Eğer mermi yan gidiyorsa bunu 90, -90 veya 180 yap.
    public float visualRotationOffset = 0f; 

    [Header("Efektler")]
    public GameObject hitEffect;

    private Transform target;
    private Rigidbody2D rb;
    private float spawnTime;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spawnTime = Time.time;
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }

        // Boss ile çarpışmayı yoksay
        GameObject boss = GameObject.FindGameObjectWithTag("Enemy");
        if (boss != null)
        {
            Collider2D myCollider = GetComponent<Collider2D>();
            Collider2D bossCollider = boss.GetComponent<Collider2D>();
            if (myCollider != null && bossCollider != null)
                Physics2D.IgnoreCollision(myCollider, bossCollider);
        }

        Destroy(gameObject, lifetime);
    }

    void FixedUpdate()
    {
        // 1. FİZİKSEL HAREKET (Her zaman kendi kırmız okunun yönüne gider)
        rb.linearVelocity = transform.right * speed;

        // 2. GÜDÜMLEME (DÖNÜŞ)
        if (Time.time > spawnTime + homingDelay)
        {
            if (target != null)
            {
                Vector2 direction = (Vector2)target.position - rb.position;
                direction.Normalize();

                float rotateAmount = Vector3.Cross(direction, transform.right).z;
                rb.angularVelocity = -rotateAmount * rotateSpeed;
            }
        }
        
        // 3. GÖRSEL DÜZELTME (Sprite Dönmesi Sorunu İçin)
        // Merminin içindeki SpriteRenderer objesini bulup onu düzeltebiliriz
        // Veya direkt bu objenin rotasyonunu offsetleyebiliriz ama o fizik yönünü bozar.
        // En temizi: Eğer mermi sprite'ı yamuk duruyorsa, Unity Editör'de Prefab'ın içine gir,
        // Child (Alt) obje olarak Sprite'ı ayarla ve orada döndür.
        // Ama kodla yapmak istersen:
        // (Bu kısım fiziksel yönü bozmadan sadece görseli döndürmek için
        // SpriteRenderer'ın ayrı bir child objede olması gerekir.
        // Eğer tek objeyse, lütfen Inspector'dan 'visualRotationOffset' ayarını 0 bırak
        // ve Sprite'ını Photoshop'ta veya Sprite Editor'de düzelt.)
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.CompareTag("Enemy") || hitInfo.CompareTag("Bullet") || hitInfo.isTrigger) return;

        if (hitInfo.CompareTag("Player"))
        {
            Player playerScript = hitInfo.GetComponent<Player>();
            if (playerScript != null)
            {
                playerScript.TakeDamage(damage, transform.position);
            }
            DestroyProjectile();
        }
        
        if (hitInfo.gameObject.layer == LayerMask.NameToLayer("Ground") || 
            hitInfo.gameObject.layer == LayerMask.NameToLayer("Default"))
        {
            DestroyProjectile();
        }
    }

    void DestroyProjectile()
    {
        if (hitEffect != null) Instantiate(hitEffect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}