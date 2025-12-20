using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    [Header("Ayarlar")]
    public int ammoAmount = 5;
    public GameObject pickupEffect;

    // Trigger yerine Collision kullanıyoruz
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // OnCollision'da 'collision.gameObject' kullanılır
        if (collision.gameObject.CompareTag("Player"))
        {
            // Player'ın içindeki (veya child'ındaki) silahı bul
            PlayerWeapon weapon = collision.gameObject.GetComponentInChildren<PlayerWeapon>();

            if (weapon != null)
            {
                weapon.AddAmmo(ammoAmount);

                if (pickupEffect != null)
                {
                    Instantiate(pickupEffect, transform.position, Quaternion.identity);
                }

                Destroy(gameObject);
            }
        }
    }
}