using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 10;
    public GameObject hitEffect; 

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.CompareTag("Player") || hitInfo.CompareTag("Bullet")) return;

        // ARTIK SADECE "ENEMYBASE" ARIYORUZ
        // AlienBug da olsa, EnemyFly da olsa, Boss da olsa hepsi EnemyBase olduğu için çalışır.
        EnemyBase enemy = hitInfo.GetComponent<EnemyBase>();
        
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            DestroyBullet();
            return;
        }

        if (hitInfo.gameObject.layer == LayerMask.NameToLayer("Ground") || hitInfo.gameObject.layer == LayerMask.NameToLayer("Default"))
        {
            DestroyBullet();
        }
    }

    void DestroyBullet()
    {
        if (hitEffect != null) 
            Instantiate(hitEffect, transform.position, Quaternion.identity);
            
        Destroy(gameObject);
    }
}