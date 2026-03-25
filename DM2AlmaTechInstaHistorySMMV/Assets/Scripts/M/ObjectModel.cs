using UnityEngine;

public class ObjectModel
{
    public float horizontalAngle; // Y
    public float verticalAngle;   // X
    public Vector2 rotation;
    public float zoom = 10f;
    public float intensity = 300f;

    public float minZoom = 5f;
    public float maxZoom = 10f;
    public float minInt = 0f;
    public float maxInt = 370f;

    public void UpdateRotation(Vector2 input, float rotationSpeed, float deltaTime)
    {
        horizontalAngle -= input.x * rotationSpeed * deltaTime;
        verticalAngle += input.y * rotationSpeed * deltaTime;
    }

    public void UpdateZoom(float input, float speed, float deltaTime)
    {
        zoom -= input * speed * deltaTime;
        zoom = Mathf.Clamp(zoom, minZoom, maxZoom);
    }

    public void UpdateLight(float lInput, float speed, float deltaTime)
    {
        intensity -= lInput * speed * deltaTime;
        intensity = Mathf.Clamp(intensity, minInt, maxInt);
    }
}
