using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static ObjectModel;

public class ObjectController : MonoBehaviour
{
    private InputSystem_Actions controls;
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

    [Header("Salto a la siguiente escena (mantener B)")]
    [Tooltip("Segundos desde que inicia la escena hasta que aparece la instrucción de B")]
    public float exitHintDelay = 30f;
    [Tooltip("Segundos que hay que mantener B presionado para cambiar de escena")]
    public float exitHoldDuration = 1.5f;
    [Tooltip("Nombre de la escena a cargar (debe estar en File > Build Settings)")]
    public string nextSceneName;
    [Tooltip("Objeto con el ícono de B. Debe estar asignado; se oculta al iniciar y se activa al cumplirse el tiempo")]
    public GameObject exitHintObject;
    [Tooltip("(Opcional) Image con Image Type = Filled para mostrar cuánto falta de la pulsación")]
    public Image exitHoldFill;

    private float _exitHintTimer = 0f;
    private float _exitHoldTimer = 0f;
    private bool _exitHintVisible = false;
    private bool _waitForRelease = false;
    private bool _exitTriggered = false;

    public NarrationController narrationController;
    private void Awake()
    {
        controls = new InputSystem_Actions();
        model = new ObjectModel();
        view = GetComponent<ObjectView>();
        audioController.PlayAll();
        view.ShowPanel(0);
        view.LoadCurrentModel();
        ResetExitHint();
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void Update()
    {
        Vector2 rotateInput = controls.ViewControls.Rotate.ReadValue<Vector2>();
        float zoomInput = controls.ViewControls.Zoom.ReadValue<float>();
        model.UpdateRotation(rotateInput, rotationSpeed, Time.deltaTime);
        model.UpdateZoom(zoomInput, zoomSpeed, Time.deltaTime);

        UpdateExitHint();
        /*
        float bumperInput = controls.ViewControls.Light.ReadValue<float>();

        // Toggle con bot�n Y
        if (controls.ViewControls.ToggleMode.triggered)
        {
            model.ToggleMode();
            view.ShowMode(model.currentMode);
        }

        


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
*/

        view.ApplyRotation(model.horizontalAngle, model.verticalAngle);
        view.ApplyZoom(model.zoom);
        view.ApplyLight(model.intensity);
        view.ApplyContrast(model.contrast);

        // Detectar ROTACI�N
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

        bool isFocused = model.zoom < 2.0f;

        if (controls.NarrationControls.Pause.triggered)
        {
            narrationController.PauseNarration();
        }

        if (controls.NarrationControls.Resume.triggered)
        {
            narrationController.ResumeNarration();
        }

        if (controls.NarrationControls.Skip.triggered)
        {
            narrationController.SkipNarration();
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

    private void UpdateExitHint()
    {
        if (_exitTriggered) return;

        float dt = Time.unscaledDeltaTime;

        // 1) Esperar a que se cumpla el tiempo
        if (!_exitHintVisible)
        {
            _exitHintTimer += dt;
            if (_exitHintTimer >= exitHintDelay) ShowExitHint();
            return;
        }

        bool pressed = controls.ViewControls.NextScene.ReadValue<float>() > 0.5f;

        // 2) Si B ya estaba presionado cuando apareció la instrucción, exigir soltarlo primero
        if (_waitForRelease)
        {
            if (!pressed) _waitForRelease = false;
            return;
        }

        // 3) Mantener presionado
        if (pressed)
        {
            _exitHoldTimer += dt;
            float required = Mathf.Max(0.01f, exitHoldDuration);
            SetExitHoldProgress(_exitHoldTimer / required);

            if (_exitHoldTimer >= required) GoToNextScene();
        }
        else if (_exitHoldTimer > 0f)
        {
            // Soltó antes de tiempo: reiniciar
            _exitHoldTimer = 0f;
            SetExitHoldProgress(0f);
        }
    }

    private void ShowExitHint()
    {
        _exitHintVisible = true;
        _waitForRelease = controls.ViewControls.NextScene.ReadValue<float>() > 0.5f;

        if (exitHintObject != null) exitHintObject.SetActive(true);
        SetExitHoldProgress(0f);
    }

    private void ResetExitHint()
    {
        _exitHintTimer = 0f;
        _exitHoldTimer = 0f;
        _exitHintVisible = false;
        _waitForRelease = false;
        _exitTriggered = false;

        if (exitHintObject != null) exitHintObject.SetActive(false);
        SetExitHoldProgress(0f);
    }

    private void SetExitHoldProgress(float progress)
    {
        if (exitHoldFill != null) exitHoldFill.fillAmount = Mathf.Clamp01(progress);
    }

    private void GoToNextScene()
    {
        if (string.IsNullOrEmpty(nextSceneName) || !Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            Debug.LogError($"[ObjectController] La escena '{nextSceneName}' no existe o no está en Build Settings.");
            _exitHoldTimer = 0f;
            SetExitHoldProgress(0f);
            return;
        }

        _exitTriggered = true;
        SceneManager.LoadScene(nextSceneName);
    }
}