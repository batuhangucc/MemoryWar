using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    [Header("Referanslar")]
    public Player playerScript; // Player scriptini buraya sürükle (veya otomatik bulur)

    [Header("Fire Ayarları")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    public float bulletSpeed = 12f;

    [Header("FirePoint Düzeltme (Offset)")]
    [Tooltip("Karakter SAĞA bakarken FirePoint nerede olsun?")]
    public Vector2 normalLocalPos;

    [Tooltip("Karakter SOLA bakarken FirePoint nerede olsun? (Bunu Play modunda ayarla)")]
    public Vector2 flippedLocalPos;

    void Awake()
    {
        // Player scripti atanmadıysa, parent objeden otomatik bul
        if (playerScript == null)
            playerScript = GetComponentInParent<Player>();

        // Başlangıçta mevcut konumu "Normal" (Sağa bakan) konum olarak kaydet
        if (firePoint != null)
        {
            normalLocalPos = firePoint.localPosition;
            
            // Flipped için varsayılan bir tahmin yapalım (Y eksenini ters çevirerek)
            // Ama sen bunu Inspector'dan ince ayar yapacaksın.
            if (flippedLocalPos == Vector2.zero)
                flippedLocalPos = new Vector2(normalLocalPos.x, -normalLocalPos.y);
        }
    }

    void Update()
    {
        // Her karede FirePoint pozisyonunu kontrol et ve düzelt
        HandleFirePointFlip();
    }

    void OnEnable()
    {
        // Event aboneliği (Input sisteminize göre burası kalabilir)
        if (playerScript != null) // Hata almamak için kontrol
            PlayerInput.OnShoot += HandleShoot;
    }

    void OnDisable()
    {
        PlayerInput.OnShoot -= HandleShoot;
    }

    // 🔥 SİHİRLİ DOKUNUŞ BURADA 🔥
    void HandleFirePointFlip()
    {
        if (playerScript == null || firePoint == null) return;

        // Player scriptindeki 'isFacingLeft' değişkenini okuyoruz
        if (playerScript.isFacingLeft)
        {
            // Sola bakıyorsa: Ayarladığın "Flipped" pozisyonuna geç
            firePoint.localPosition = flippedLocalPos;
        }
        else
        {
            // Sağa bakıyorsa: Orijinal "Normal" pozisyona geç
            firePoint.localPosition = normalLocalPos;
        }
    }

    void HandleShoot(Vector2 mouseWorldPos)
    {
        if (firePoint == null) return;

        Vector2 firePos = firePoint.position;
        Vector2 dir = (mouseWorldPos - firePos).normalized;

        // --- MERMİ ROTASYONU ---
        // Mermi gidiş yönüne baksın istiyorsan bu hesabı yapmalısın:
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        // Mermiyi oluştur
        GameObject bullet = Instantiate(
            bulletPrefab,
            firePoint.position,
            rotation // Mermiyi hesaplanan açıyla doğur
        );

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = dir * bulletSpeed;
            rb.freezeRotation = true; // Fizik motoru mermiyi döndürmesin, bizim açımızda kalsın
        }
    }
}