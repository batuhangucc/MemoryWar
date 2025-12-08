using UnityEngine;

public class PlayerTilt : MonoBehaviour
{
    public Transform graphics;      // Graphics child’ı
    public float maxTilt = 10f;     // max derece (sağa/sola eğim)
    public float tiltSpeed = 10f;   // ne kadar yumuşak dönecek
    public SpriteRenderer sr;       // karakterin SpriteRenderer'ı

    Camera cam;
    Vector2 currentDir = Vector2.right;

    void Awake()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (graphics == null || sr == null) return;

        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Vector2 dir = (mouseWorld - graphics.position).normalized;

        // Sağ/sol yönü belirle (flip)
        bool lookRight = dir.x >= 0f;
        sr.flipX = !lookRight;  // sprite sağa bakacak şekilde çizildiyse bu iş görür

        // Yalnızca dikey farktan “tilt” üretelim
        // Yukarıya bakıyorsa pozitif, aşağıya bakıyorsa negatif gibi düşün
        float targetTilt = Mathf.Clamp(dir.y * 30f, -maxTilt, maxTilt);

        // Şu anki lokal Z açısını -180..180 aralığına çevir
        float currentZ = graphics.localEulerAngles.z;
        if (currentZ > 180f) currentZ -= 360f;

        float newZ = Mathf.Lerp(currentZ, targetTilt, Time.deltaTime * tiltSpeed);

        graphics.localRotation = Quaternion.Euler(0f, 0f, newZ);
    }
}