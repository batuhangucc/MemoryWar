using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    public Transform cam;          
    public float strength = 0.2f;  

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

        
        float parallaxX = camDelta.x * strength;
        float parallaxY = camDelta.y * strength * 0.0f; 

        transform.position = new Vector3(
            startPos.x + parallaxX,
            startPos.y + parallaxY,
            startPos.z
        );
    }
}