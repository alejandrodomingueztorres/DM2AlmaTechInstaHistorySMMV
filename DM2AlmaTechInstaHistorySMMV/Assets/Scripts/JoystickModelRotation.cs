using UnityEngine;
using UnityEngine.InputSystem;

public class JoystickModelRotation : MonoBehaviour
{
    [Header("Pivotes de rotación")]
    [SerializeField] private Transform horizontalPivot; // rota en Y
    [SerializeField] private Transform verticalPivot;   // rota en X

    [Header("Velocidad de rotación")]
    [SerializeField] private float horizontalSpeed = 120f;
    [SerializeField] private float verticalSpeed = 80f;

    [Header("Zona muerta del joystick")]
    [SerializeField] private float deadZone = 0.15f;

    [Header("Límites verticales")]
    [SerializeField] private bool useVerticalLimits = true;
    [SerializeField] private float minVerticalAngle = -35f;
    [SerializeField] private float maxVerticalAngle = 35f;

    private InputSystem_Actions controls;
    private Vector2 lookInput;

    private float currentVerticalAngle;

    private void Awake()
    {
        controls = new InputSystem_Actions();

        controls.Player.Look.performed += OnLookPerformed;
        controls.Player.Look.canceled += OnLookCanceled;
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Player.Look.performed -= OnLookPerformed;
        controls.Player.Look.canceled -= OnLookCanceled;
        controls.Disable();
        controls.Dispose();
    }

    private void Start()
    {
        if (horizontalPivot == null)
            horizontalPivot = transform;

        if (verticalPivot == null)
            verticalPivot = transform;

        currentVerticalAngle = NormalizeAngle(verticalPivot.localEulerAngles.x);
    }

    private void Update()
    {
        if (horizontalPivot == null || verticalPivot == null) return;

        float inputX = lookInput.x;
        float inputY = lookInput.y;

        if (Mathf.Abs(inputX) < deadZone) inputX = 0f;
        if (Mathf.Abs(inputY) < deadZone) inputY = 0f;

        // Rotación horizontal corregida
        float horizontalRotation = -inputX * horizontalSpeed * Time.deltaTime;
        horizontalPivot.Rotate(0f, horizontalRotation, 0f, Space.World);

        // Rotación vertical sin inversión
        currentVerticalAngle += inputY * verticalSpeed * Time.deltaTime;

        if (useVerticalLimits)
        {
            currentVerticalAngle = Mathf.Clamp(currentVerticalAngle, minVerticalAngle, maxVerticalAngle);
        }

        verticalPivot.localRotation = Quaternion.Euler(currentVerticalAngle, 0f, 0f);
    }

    private void OnLookPerformed(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    private void OnLookCanceled(InputAction.CallbackContext context)
    {
        lookInput = Vector2.zero;
    }

    private float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle > 180f) angle -= 360f;
        return angle;
    }
}