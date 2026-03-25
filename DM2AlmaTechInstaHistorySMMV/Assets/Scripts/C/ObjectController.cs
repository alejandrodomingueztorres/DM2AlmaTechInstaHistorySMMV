using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectController : MonoBehaviour
{
    private Explore controls;
    public ObjectModel model;
    public ObjectView view;
    public float lChangeSpeed = 100f;

    public float rotationSpeed = 100f;
    public float zoomSpeed = 5f;

    private void Awake()
    {
        controls = new Explore();
        model = new ObjectModel();
        view = GetComponent<ObjectView>();
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void Update()
    {
        Vector2 rotateInput = controls.ViewControls.Rotate.ReadValue<Vector2>();
        float zoomInput = controls.ViewControls.Zoom.ReadValue<float>();
        float lightInput = controls.ViewControls.Light.ReadValue<float>();

        model.UpdateRotation(rotateInput, rotationSpeed, Time.deltaTime);
        model.UpdateZoom(zoomInput, zoomSpeed, Time.deltaTime);
        model.UpdateLight(lightInput, lChangeSpeed, Time.deltaTime);

        view.ApplyRotation(model.horizontalAngle, model.verticalAngle);
        view.ApplyZoom(model.zoom);
        view.ApplyLight(model.intensity);
    }
}