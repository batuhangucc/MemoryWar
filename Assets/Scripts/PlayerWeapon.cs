using UnityEngine;
using UnityEngine.UI; // Image bileşeni için gerekli kütüphane

public class PlayerWeapon : MonoBehaviour
{
    [Header("Referanslar")]
    public Player playerScript; // Player scriptini buraya sürükle (veya otomatik bulur)

    [Header("UI Ayarları")]
    [Tooltip("Canvas üzerindeki 'Filled' yaptığımız mermi görselini buraya sürükle")]
    public Image ammoFillImage; 

    [Header("Mermi Kapasitesi")]
    public int maxAmmo = 30; // Toplam mermi kapasitesi
    private int currentAmmo; // Şu anki mermi

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

        // --- MERMİ BAŞLANGIÇ AYARLARI ---
        currentAmmo = maxAmmo; // Oyuna full mermiyle başla
        UpdateAmmoUI();        // UI'ı güncelle

        // --- FIREPOINT OFFSET AYARLARI ---
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

    void OnEnable()
    {
        // Event aboneliği
        if (playerScript != null) 
            PlayerInput.OnShoot += HandleShoot;
    }

    void OnDisable()
    {
        PlayerInput.OnShoot -= HandleShoot;
    }

    void Update()
    {
        // Her karede FirePoint pozisyonunu kontrol et ve düzelt
        HandleFirePointFlip();
    }

    // 🔥 SİHİRLİ DOKUNUŞ: FirePoint Pozisyonunu Düzeltme
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

        // --- MERMİ KONTROLÜ ---
        if (currentAmmo <= 0)
        {
            Debug.Log("Mermi Bitti! Tık sesi çalınabilir.");
            return; // Mermi yoksa ateş etme, fonksiyondan çık
        }

        // Mermiyi azalt ve UI'ı güncelle
        currentAmmo--;
        UpdateAmmoUI();

        // --- ATEŞ ETME MANTIĞI ---
        Vector2 firePos = firePoint.position;
        Vector2 dir = (mouseWorldPos - firePos).normalized;

        // Mermi Rotasyonu
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        // Mermiyi oluştur
        GameObject bullet = Instantiate(
            bulletPrefab,
            firePoint.position,
            rotation 
        );

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = dir * bulletSpeed;
            rb.freezeRotation = true; 
        }
    }

    // --- UI GÜNCELLEME YARDIMCISI ---
    void UpdateAmmoUI()
    {
        if (ammoFillImage != null)
        {
            // Matematik: (Mevcut / Maksimum) -> Örn: 15 / 30 = 0.5 (Yarısı dolu)
            ammoFillImage.fillAmount = (float)currentAmmo / maxAmmo;
        }
    }

    // Mermi doldurmak istersen bu fonksiyonu dışarıdan çağırabilirsin (Örn: AmmoBox alınca)
    public void ReloadAmmo(int amount)
    {
        currentAmmo += amount;
        if (currentAmmo > maxAmmo) currentAmmo = maxAmmo;
        UpdateAmmoUI();
    }
    public void AddAmmo(int amount)
{
    currentAmmo += amount;

    // Kapasiteyi aşmasın
    if (currentAmmo > maxAmmo) 
    {
        currentAmmo = maxAmmo;
    }

    // UI'ı hemen güncelle
    UpdateAmmoUI();
    
    Debug.Log("Mermi alındı! Yeni mermi: " + currentAmmo);
}
}