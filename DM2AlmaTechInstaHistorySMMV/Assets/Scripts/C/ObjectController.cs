using UnityEngine;
using UnityEngine.InputSystem;
using static ObjectModel;

public class ObjectController : MonoBehaviour
{
    private Explore controls;
    public ObjectModel model;
    public ObjectView view;
    public float lChangeSpeed = 100f;

    public float rotationSpeed = 100f;
    public float zoomSpeed = 5f;

    private float lastLight;
    private float lastContrast;

    public AudioController audioController;

    private int tutorialStep = 0;

    private bool usedRotate = false;
    private bool usedZoom = false;
    private bool usedLight = false;
    private bool usedToggle = false;
    private bool usedFinal = false;

    private void Awake()
    {
        controls = new Explore();
        model = new ObjectModel();
        view = GetComponent<ObjectView>();
        audioController.PlayAll();
        view.ShowPanel(0);
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void Update()
    {
        Vector2 rotateInput = controls.ViewControls.Rotate.ReadValue<Vector2>();
        float zoomInput = controls.ViewControls.Zoom.ReadValue<float>();
        float bumperInput = controls.ViewControls.Light.ReadValue<float>();

        // Toggle con botón Y
        if (controls.ViewControls.ToggleMode.triggered)
        {
            model.ToggleMode();
            view.ShowMode(model.currentMode);
        }

        model.UpdateRotation(rotateInput, rotationSpeed, Time.deltaTime);
        model.UpdateZoom(zoomInput, zoomSpeed, Time.deltaTime);


        if (model.currentMode == ControlMode.Light)
        {
            float prev = model.intensity;

            model.UpdateLight(bumperInput, lChangeSpeed, Time.deltaTime);

            if (!Mathf.Approximately(prev, model.intensity))
            {
                view.ShowLightUI(model.intensity);
            }
        }
        else
        {
            float prev = model.contrast;

            model.UpdateContrast(bumperInput, 50f, Time.deltaTime);

            if (!Mathf.Approximately(prev, model.contrast))
            {
                view.ShowContrastUI(model.contrast);
            }
        }

        view.ApplyRotation(model.horizontalAngle, model.verticalAngle);
        view.ApplyZoom(model.zoom);
        view.ApplyLight(model.intensity);
        view.ApplyContrast(model.contrast);

        // Detectar ROTACIÓN
        if (!usedRotate && rotateInput.magnitude > 0.1f)
        {
            usedRotate = true;
            NextStep();
        }

        // Detectar ZOOM
        if (!usedZoom && Mathf.Abs(zoomInput) > 0.1f)
        {
            usedZoom = true;
            NextStep();
        }

        // Detectar LIGHT / CONTRAST (bumpers)
        if (!usedLight && Mathf.Abs(bumperInput) > 0.1f)
        {
            usedLight = true;
            NextStep();
        }

        // Detectar TOGGLE (botón Y)
        if (!usedToggle && controls.ViewControls.ToggleMode.triggered)
        {
            usedToggle = true;
            NextStep();
        }

        // Detectar último uso de bumpers
        if (tutorialStep == 4 && !usedFinal && Mathf.Abs(bumperInput) > 0.1f)
        {
            usedFinal = true;
            NextStep();
        }

    }

    private void NextStep()
    {
        tutorialStep++;

        if (tutorialStep < view.tutorialPanels.Length)
        {
            view.ShowPanel(tutorialStep);
        }
        else
        {
            view.HideAllPanels();
        }
    }
}