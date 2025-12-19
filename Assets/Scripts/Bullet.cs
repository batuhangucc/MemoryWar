using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
    [Header("Mermi Ayarları")]
    public int damage = 1;          // Düşmana kaç hasar verecek?
    public float lifetime = 3f;     // Hiçbir şeye çarpmazsa kaç saniye sonra yok olsun?

    [Header("Görsel Efekt (Opsiyonel)")]
    public GameObject hitEffectPrefab; // Çarpınca çıkacak kıvılcım/patlama efekti

    void Start()
    {
        // Performans için: Eğer mermi boşa giderse sonsuza kadar sahnede kalmasın
        Destroy(gameObject, lifetime);
    }

    // Is Trigger açık olduğu için OnTriggerEnter2D kullanıyoruz
    void OnTriggerEnter2D(Collider2D hitInfo)
{
    // 1. Kendi sahibine (Player) çarpma -> Yoksay
    if (hitInfo.CompareTag("Player")) return;
    
    // 2. Başka mermilere çarpma -> Yoksay
    if (hitInfo.CompareTag("Bullet")) return;

    // 3. Loot (Mermi kutusu) gibi triggerlara çarpma -> Yoksay
    // (Optimizasyon: GetComponent maliyetlidir, mümkünse buna da Tag verip CompareTag kullanmanı öneririm)
    if (hitInfo.GetComponent<AmmoPickup>() != null) return;

    // --- DÜŞMAN ETKİLEŞİMİ ---
    
    // Eğer çarptığımız şey bir Düşman ise:
    if (hitInfo.TryGetComponent<EnemyBase>(out EnemyBase enemy))
    {
        enemy.TakeDamage(damage);
        DestroyBullet(); // Mermiyi yok et
        return;          // Fonksiyondan çık (Aşağıdaki koda gitme)
    }

    // --- DUVAR / ZEMİN ETKİLEŞİMİ (Eksik Olan Parça) ---
    
    // Buraya kadar kod "return" olmadıysa, demek ki çarptığımız şey:
    // Player DEĞİL, Bullet DEĞİL, Loot DEĞİL, Düşman DEĞİL.
    // O zaman bu bir DUVAR veya ZEMİNDİR. Mermiyi yok et.
    DestroyBullet();
}

    void DestroyBullet()
    {
        // Eğer bir patlama efekti atadıysan onu oluştur
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }

        // Mermi objesini yok et
        Destroy(gameObject);
    }
}