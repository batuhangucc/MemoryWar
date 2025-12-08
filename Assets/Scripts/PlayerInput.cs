using System;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    // Mouse dünyadaki konumu ile ateş event'i
    public static event Action<Vector2> OnShoot;

    Camera _cam;

    void Awake()
    {
        _cam = Camera.main;
    }

    void Update()
    {
        // Sol tık (Fire1) gelince event fırlat
        if (Input.GetButtonDown("Fire1"))
        {
            Vector3 mouseScreen = Input.mousePosition;
            Vector3 mouseWorld = _cam.ScreenToWorldPoint(mouseScreen);
            mouseWorld.z = 0f;

            OnShoot?.Invoke(mouseWorld);
        }
    }
}