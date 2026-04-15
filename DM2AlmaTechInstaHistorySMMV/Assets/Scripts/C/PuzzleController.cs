using UnityEngine;
using UnityEngine.InputSystem;

public class PuzzleController : MonoBehaviour
{
    [Header("MVC")]
    public PuzzleModel model;
    public PuzzleView view;

    [Header("References")]
    public Camera cam;

    [Header("Settings")]
    public float snapDistance = 0.2f;
    public float moveSpeed = 10f;
    public float rotationSpeed = 150f;

    [Header("Cursor Settings")]
    public float cursorSpeed = 800f;

    // Input values
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool grabPressed;
    private bool releasePressed;

    // Selection
    private GameObject selectedPiece;
    private PieceModel selectedModel;
    private PieceView selectedView;
    private InputSystem_Actions inputActions;

    // Posición de la pieza
    private Vector3 currentPiecePosition;

    // Cursor virtual (posición en pantalla)
    private Vector2 cursorPosition;

    private Texture2D whiteTexture;

    void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    void Start()
    {
        cursorPosition = new Vector2(Screen.width / 2f, Screen.height / 2f);

        whiteTexture = new Texture2D(1, 1);
        whiteTexture.SetPixel(0, 0, Color.white);
        whiteTexture.Apply();
    }

    void OnEnable()
    {
        inputActions.Puzzle.Enable();

        inputActions.Puzzle.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Puzzle.Move.canceled += ctx => moveInput = Vector2.zero;

        inputActions.Puzzle.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        inputActions.Puzzle.Look.canceled += ctx => lookInput = Vector2.zero;

        inputActions.Puzzle.Grab.performed += ctx => grabPressed = true;
        inputActions.Puzzle.Release.performed += ctx => releasePressed = true;
    }

    void OnDisable()
    {
        inputActions.Puzzle.Disable();
    }

    void Update()
    {
        UpdateCursor();
        HandleSelection();
        HandleMovement();

        grabPressed = false;
        releasePressed = false;
    }

    // =========================
    // CURSOR CONTROL (NUEVO)
    // =========================
    void UpdateCursor()
    {
        // Mover cursor con stick derecho
        cursorPosition += lookInput * cursorSpeed * Time.deltaTime;

        // Limitar a pantalla
        cursorPosition.x = Mathf.Clamp(cursorPosition.x, 0, Screen.width);
        cursorPosition.y = Mathf.Clamp(cursorPosition.y, 0, Screen.height);
    }

    // =========================
    // SELECCIÓN (MODIFICADO)
    // =========================
    void HandleSelection()
    {
        if (grabPressed)
        {
            Ray ray = cam.ScreenPointToRay(cursorPosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                PieceView pv = hit.collider.GetComponent<PieceView>();

                if (pv != null)
                {
                    selectedPiece = hit.collider.gameObject;
                    selectedView = pv;

                    selectedModel = model.pieces.Find(p => p.id == selectedPiece.name);

                    if (selectedModel == null)
                    {
                        Debug.LogWarning($"No se encontró PieceModel con id '{selectedPiece.name}'");
                        selectedPiece = null;
                        selectedView = null;
                        return;
                    }

                    currentPiecePosition = selectedPiece.transform.position;
                }
            }
        }

        if (releasePressed && selectedPiece != null)
        {
            TryPlacePiece();
            selectedPiece = null;
            selectedModel = null;
            selectedView = null;
        }
    }

    // =========================
    // MOVIMIENTO
    // =========================
    void HandleMovement()
    {
        if (selectedPiece == null) return;

        if (moveInput.sqrMagnitude > 0.01f)
        {
            Vector3 camForward = cam.transform.forward;
            Vector3 camRight = cam.transform.right;

            camForward.y = 0f;
            camRight.y = 0f;

            camForward.Normalize();
            camRight.Normalize();

            Vector3 moveDirection = (camForward * moveInput.y + camRight * moveInput.x);
            currentPiecePosition += moveDirection * moveSpeed * Time.deltaTime;
        }

        selectedPiece.transform.position = Vector3.Lerp(
            selectedPiece.transform.position,
            currentPiecePosition,
            Time.deltaTime * moveSpeed
        );

        float rot = lookInput.x;
        selectedPiece.transform.Rotate(Vector3.up * rot * rotationSpeed * Time.deltaTime);
    }

    // =========================
    // COLOCACIÓN
    // =========================
    void TryPlacePiece()
    {
        if (selectedModel == null || selectedModel.targetTransform == null)
        {
            Debug.LogWarning("TryPlacePiece: selectedModel o targetTransform es null.");
            return;
        }

        float dist = Vector3.Distance(
            selectedPiece.transform.position,
            selectedModel.targetTransform.position
        );

        if (dist < snapDistance)
        {
            selectedPiece.transform.position = selectedModel.targetTransform.position;
            selectedPiece.transform.rotation = selectedModel.targetTransform.rotation;

            selectedModel.isPlaced = true;
            selectedView.PlayCorrectFeedback();

            CheckSpecialPairs(selectedModel.pairType);

            if (model.AreAllPlaced())
            {
                OnAllPiecesPlaced();
            }
        }
        else
        {
            selectedView.PlayWrongFeedback();
        }
    }

    void CheckSpecialPairs(SpecialPairType type)
    {
        if (type == SpecialPairType.None) return;

        if (model.CheckSpecialPair(type))
        {
            Debug.Log("Par especial completado: " + type);
        }
    }

    void OnAllPiecesPlaced()
    {
        view.OnPuzzleCompleted();
        OnPuzzleCompleted();
    }

    void OnPuzzleCompleted()
    {
        // Lógica futura
    }

    // =========================
    // DEBUG VISUAL (OPCIONAL)
    // =========================
    void OnGUI()
    {
        float size = 14f;
        float thickness = 2f;

        Color previous = GUI.color;
        GUI.color = Color.green;

        // Horizontal
        GUI.DrawTexture(new Rect(cursorPosition.x - size / 2, Screen.height - cursorPosition.y, size, thickness), whiteTexture);

        // Vertical
        GUI.DrawTexture(new Rect(cursorPosition.x, Screen.height - cursorPosition.y - size / 2, thickness, size), whiteTexture);

        GUI.color = previous;
    }
}