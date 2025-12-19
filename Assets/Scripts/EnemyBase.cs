using UnityEngine;

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

    protected Animator anim;
    protected Rigidbody2D rb;
    protected bool isDead = false;

    protected virtual void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
    }

    // Hasar alma fonksiyonu (EnemyFly bunu override edip STUN ekleyecek)
    public virtual void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        // --- DEĞİŞİKLİK BURADA ---
        // Senin Animator pencerende parametre adı "Damage" olduğu için bunu kullanıyoruz.
        // Bu kod çalıştığında düşman beyaz yanıp sönecek (veya damage animasyonu girecek).
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
        
        // DİKKAT: Animator penceresinde "die" isminde bir Trigger oluşturduğundan emin ol!
        // (Büyük küçük harf duyarlıdır: "Die" mı "die" mı kontrol et)
        anim.SetTrigger("die"); 

        // Loot düşür
        TryDropLoot();

        // Fiziği kapat ki ceset aşağı düşmesin veya player çarpmasın
        rb.simulated = false; 
        // Veya sadece collider'ı kapatabilirsin: GetComponent<Collider2D>().enabled = false;

        // Cesedi 3 saniye sonra sahneden sil (Animasyon bitsin diye bekliyoruz)
        Destroy(gameObject, 3f);
    }

    void TryDropLoot()
    {
        if (lootPrefab != null && Random.Range(0, 100) <= dropChance)
        {
            Instantiate(lootPrefab, transform.position, Quaternion.identity);
        }
    }
}