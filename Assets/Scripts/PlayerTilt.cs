using UnityEngine;

public class PlayerTilt : MonoBehaviour
{
    public Transform graphics;      // Graphics child’ı
    public float maxTilt = 8f;      // daha küçük = ayak yerden kopmaz
    public float tiltSpeed = 10f;

    Camera cam;

    void Awake()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (graphics == null) return;

        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Vector2 dir = (mouseWorld - graphics.position).normalized;

        // SADECE dikey farktan tilt üret
        float targetTilt = Mathf.Clamp(dir.y * 25f, -maxTilt, maxTilt);

        float currentZ = graphics.localEulerAngles.z;
        if (currentZ > 180f) currentZ -= 360f;

        float newZ = Mathf.Lerp(
            currentZ,
            targetTilt,
            Time.deltaTime * tiltSpeed
        );

        graphics.localRotation = Quaternion.Euler(0f, 0f, newZ);
    }
}