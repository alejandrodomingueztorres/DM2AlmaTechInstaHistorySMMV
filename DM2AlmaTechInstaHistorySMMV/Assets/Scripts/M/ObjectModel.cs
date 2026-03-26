using UnityEngine;

public class ObjectModel
{
    public float horizontalAngle;
    public float verticalAngle;

    public float zoom = 10f;
    public float intensity = 300f;
    public float contrast = 0f;

    public ControlMode currentMode = ControlMode.Light;

    public float minZoom = 5f;
    public float maxZoom = 10f;

    public float minInt = 0f;
    public float maxInt = 370f;

    public float minContrast = 0f;
    public float maxContrast = 100f;

    public void ToggleMode()
    {
        currentMode = currentMode == ControlMode.Light
            ? ControlMode.Contrast
            : ControlMode.Light;
    }

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

    public void UpdateLight(float input, float speed, float deltaTime)
    {
        intensity -= input * speed * deltaTime;
        intensity = Mathf.Clamp(intensity, minInt, maxInt);
    }

    public void UpdateContrast(float input, float speed, float deltaTime)
    {
        contrast -= input * speed * deltaTime;
        contrast = Mathf.Clamp(contrast, minContrast, maxContrast);
    }


    public enum ControlMode
    {
        Light,
        Contrast
    }
}
