using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    public Transform cam;          // Main Camera
    public float strength = 0.2f;  // 0 = sabit, 1 = kamera ile aynı hız

    Vector3 startPos;
    Vector3 startCamPos;

    void Start()
    {
        if (cam == null)
            cam = Camera.main.transform;

        startPos = transform.position;
        startCamPos = cam.position;
    }

    void LateUpdate()
    {
        Vector3 camDelta = cam.position - startCamPos;

        // Sadece X’de parallax istiyorsan Y’yi 0 yap
        float parallaxX = camDelta.x * strength;
        float parallaxY = camDelta.y * strength * 0.0f; // şimdilik 0, istersen 1 yaparsın

        transform.position = new Vector3(
            startPos.x + parallaxX,
            startPos.y + parallaxY,
            startPos.z
        );
    }
}