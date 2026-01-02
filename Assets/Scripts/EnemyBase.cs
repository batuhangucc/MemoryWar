using UnityEngine;
using System.Collections; // Coroutine için gerekli

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyBase : MonoBehaviour
{
    [Header("Temel Ayarlar")]
    public int maxHealth = 3;
    protected int currentHealth;

    [Header("Loot (Ganimet)")]
    public GameObject lootPrefab; // Mermi prefabı buraya
    [Range(0, 100)] public float dropChance = 50f; // Düşme ihtimali %

    // Çocuk sınıflar (AlienBug, EnemyFly) erişebilsin diye protected
    protected Animator anim;
    protected Rigidbody2D rb;
    protected bool isDead = false;

    protected virtual void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
    }

    public virtual void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        // Hasar animasyonu
        if (anim != null)
        {
            anim.SetTrigger("Damage");
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        isDead = true;
        
        // DİKKAT: Animator'daki Trigger adının "Die" (Büyük harf) olduğundan emin ol.
        // Eğer küçükse burayı "die" yap.
        anim.SetTrigger("Die"); 

        TryDropLoot();

        // Fiziği tamamen kapat
        rb.simulated = false; 
        GetComponent<Collider2D>().enabled = false; // Mermiler artık içinden geçer

        // Direkt Destroy etmek yerine, FadeOut (Yavaşça Silinme) başlatıyoruz
        StartCoroutine(FadeOutAndDestroy());
    }

    IEnumerator FadeOutAndDestroy()
    {
        // 1. Ölüm animasyonunun oynaması için biraz bekle (Örn: 0.5 saniye)
        yield return new WaitForSeconds(0.5f);

        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            // 2. Yavaşça şeffaflaştır (Alpha değerini düşür)
            float alpha = 1f;
            while (alpha > 0)
            {
                alpha -= Time.deltaTime * 2f; // Silinme hızı
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);
                yield return null; // Bir sonraki kareyi bekle
            }
        }

        // 3. Tamamen görünmez olunca yok et
        Destroy(gameObject);
    }

    void TryDropLoot()
    {
        if (lootPrefab != null && Random.Range(0, 100) <= dropChance)
        {
            Instantiate(lootPrefab, transform.position, Quaternion.identity);
        }
    }
}