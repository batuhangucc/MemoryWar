using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform;

    [Header("Parallax Ayarları")]
    [Range(0f, 1f)]
    [SerializeField] private float parallaxMultiplier = 0.5f;
    
    [Tooltip("Kamera yukarı-aşağı hareket ettiğinde bu layer da etkilensin mi?")]
    [SerializeField] private bool affectVertical = false;

    private Vector3 _lastCameraPosition;

    private void Awake()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        _lastCameraPosition = cameraTransform.position;
    }

    private void LateUpdate()
    {
        Vector3 camDelta = cameraTransform.position - _lastCameraPosition;

        float moveX = camDelta.x * parallaxMultiplier;
        float moveY = affectVertical ? camDelta.y * parallaxMultiplier : 0f;

        transform.position += new Vector3(moveX, moveY, 0f);

        // Z eksenini hiç bozmuyoruz
        _lastCameraPosition = cameraTransform.position;
    }
}