using UnityEngine;

public class ObjectView : MonoBehaviour
{
    public Transform horizontalPivot; // Y world
    public Transform verticalPivot;   // X local
    public Transform targetCamera;
    public Light Spotlight;

    public void ApplyRotation(float horizontal, float vertical)
    {
        if (horizontalPivot == null || verticalPivot == null) return;

        // Rotación horizontal (WORLD)
        horizontalPivot.rotation = Quaternion.Euler(0f, horizontal, 0f);

        // Rotación vertical (LOCAL)
        verticalPivot.localRotation = Quaternion.Euler(vertical, 0f, 0f);
    }

    public void ApplyZoom(float zoom)
    {
        if (targetCamera != null)
        {
            targetCamera.localPosition = new Vector3(0, 0, -zoom);
        }
    }

    public void ApplyLight(float light)
    {
        if (Spotlight != null)
        { 
            Spotlight.intensity = light;
        }

    }
}