using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public Transform target;       
    public float followSpeedX = 5f;
    public float followSpeedY = 2f;   // Y ekseninde daha yumuşak takip
    public Vector2 offset = new Vector2(0f, 0f);

    public float minY = -999f;        // Kameranın inebileceği minimum seviye (istersen kullan)
    public float maxY = 999f;         // Kameranın çıkabileceği maksimum seviye

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 currentPos = transform.position;

        float targetX = Mathf.Lerp(currentPos.x, target.position.x + offset.x, followSpeedX * Time.deltaTime);

        // Yumuşak Y takibi
        float targetY = Mathf.Lerp(currentPos.y, target.position.y + offset.y, followSpeedY * Time.deltaTime);

        // Kameranın gereksiz yukarı gitmesini engellemek istersen:
        targetY = Mathf.Clamp(targetY, minY, maxY);

        transform.position = new Vector3(targetX, targetY, currentPos.z);
    }
}