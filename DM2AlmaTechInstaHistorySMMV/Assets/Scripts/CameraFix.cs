using UnityEngine;

public class CameraFix : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Camera cam = GetComponent<Camera>();
        Matrix4x4 m = cam.projectionMatrix;
        m[0, 0] = -m[0, 0];
        cam.projectionMatrix = m;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
