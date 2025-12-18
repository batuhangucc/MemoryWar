using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    [Header("Fire Ayarları")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    public float bulletSpeed = 12f;

    void OnEnable()
    {
        PlayerInput.OnShoot += HandleShoot;
    }

    void OnDisable()
    {
        PlayerInput.OnShoot -= HandleShoot;
    }

    void HandleShoot(Vector2 mouseWorldPos)
    {
        Vector2 firePos = firePoint.position;
        Vector2 dir = (mouseWorldPos - firePos).normalized;

        // ❌ SİLAH DÖNMEZ
        // transform.rotation = ...

        // Mermi oluştur
        GameObject bullet = Instantiate(
            bulletPrefab,
            firePoint.position,
            Quaternion.identity
        );

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = dir * bulletSpeed;
            rb.freezeRotation = true;
        }
    }
}