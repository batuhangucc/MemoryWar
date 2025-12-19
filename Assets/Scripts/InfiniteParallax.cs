using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class InfiniteParallax : MonoBehaviour
{
    [Header("Ayarlar")]
    public Transform cam;
    [Range(0f, 1f)]
    public float parallaxEffect; // 1 = Kamera ile aynı hız (sabit), 0 = Hiç hareket etmez (uzak)
    
    private float length;
    private float startpos;

    void Start()
    {
        if (cam == null)
            cam = Camera.main.transform;

        startpos = transform.position.x;
        
        // Sprite'ın genişliğini otomatik alıyoruz
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void LateUpdate() // Titremeyi önlemek için FixedUpdate yerine LateUpdate
    {
        // Kameranın ne kadar ilerlediğini (parallax etkisi olmadan) hesapla
        float temp = (cam.position.x * (1 - parallaxEffect));
        
        // Objeyi ne kadar hareket ettireceğimizi hesapla
        float dist = (cam.position.x * parallaxEffect);

        // Yeni pozisyonu uygula (Y eksenini koruyarak)
        transform.position = new Vector3(startpos + dist, transform.position.y, transform.position.z);

        // --- SONSUZ DÖNGÜ MANTIĞI ---
        // Eğer kamera, sprite'ın boyu kadar ilerlediyse, başlangıç noktasını kaydır.
        if (temp > startpos + length) 
        {
            startpos += length;
        }
        else if (temp < startpos - length)
        {
            startpos -= length;
        }
    }
}