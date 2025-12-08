using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerWeapon : MonoBehaviour
{
    [Header("Fire Ayarları")]
    public Transform firePoint;      // Namlu ucu
    public GameObject bulletPrefab;  // Mermi prefabı
    public float bulletSpeed = 12f;

    SpriteRenderer sr;

    void OnEnable()
    {
        PlayerInput.OnShoot += HandleShoot;
    }

    void OnDisable()
    {
        PlayerInput.OnShoot -= HandleShoot;
    }

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void HandleShoot(Vector2 mouseWorldPos)
{
    // 1) Mouse yönüne göre silahı döndür
    Vector2 firePos = firePoint.position;
    Vector2 dir = (mouseWorldPos - firePos).normalized;

    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    transform.rotation = Quaternion.Euler(0, 0, angle);

    // 2) Mermiyi oluştururken ROTASYONU SABİT TUT
    GameObject bullet = Instantiate(
        bulletPrefab,
        firePoint.position,
        Quaternion.identity   // <-- her zaman 0 derece
    );

    // 3) Sadece hız ver, rotation yok
    Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
    if (rb != null)
    {
        rb.linearVelocity = dir * bulletSpeed;

        // Dönmesini tamamen kilitlemek için:
        rb.freezeRotation = true;
    }
}
}