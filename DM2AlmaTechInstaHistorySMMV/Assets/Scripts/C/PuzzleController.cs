using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PuzzleController : MonoBehaviour
{
    [Header("MVC")]
    public PuzzleModel model;
    public PuzzleView view;

    [Header("References")]
    public Camera cam;

    // Referencia al contenedor principal de las piezas del puzzle
    public Transform piezasRoot; 


    [Header("Narraciones")]
    public AudioSource audioBase;
    public AudioSource audioCuerpo;
    public AudioSource audioCuello;
    public AudioSource audioBorde;
    public AudioSource audioSimbolo;
    public AudioSource audioPuntos;

    // Controla si una narración está activa para bloquear la interacción temporalmente
    private bool narracionActiva = false;


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

        if (!narracionActiva)
        {
            HandleSelection();
            HandleMovement();
        }

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

                    // Bloquea la selección de piezas ya colocadas para que no puedan moverse otra vez.
                    if (selectedModel.isPlaced)
                    {
                        selectedPiece = null;
                        selectedView = null;
                        selectedModel = null;
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
    // Verifica si se completó una sección especial y dispara su narración correspondiente
    void CheckSpecialPairs(SpecialPairType type)
    {
        if (type == SpecialPairType.None) return;

        if (model.CheckSpecialPair(type))
        {
            Debug.Log("Par especial completado: " + type);

            if (!narracionActiva)
            {
                if (type == SpecialPairType.Symbol)
                {
                    StartCoroutine(ReproducirNarracionConIluminacion(type, audioSimbolo));
                }
                else if (type == SpecialPairType.Dots)
                {
                    StartCoroutine(ReproducirNarracionConIluminacion(type, audioPuntos));
                }
                else if (type == SpecialPairType.Base)
                {
                    StartCoroutine(ReproducirNarracionConIluminacion(type, audioBase));
                }
                else if (type == SpecialPairType.Body)
                {
                    StartCoroutine(ReproducirNarracionConIluminacion(type, audioCuerpo));
                }
                else if (type == SpecialPairType.Neck)
                {
                    StartCoroutine(ReproducirNarracionConIluminacion(type, audioCuello));
                }
                else if (type == SpecialPairType.Lip)
                {
                    StartCoroutine(ReproducirNarracionConIluminacion(type, audioBorde));
                }
            }
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

    // Reproduce la narración de una sección y resalta visualmente sus piezas mientras dura el audio
    IEnumerator ReproducirNarracionConIluminacion(SpecialPairType type, AudioSource audio)
    {
        narracionActiva = true;

        PieceView[] piezasSeccion = ObtenerPieceViewsPorTipo(type);

        foreach (PieceView pv in piezasSeccion)
        {
            if (pv != null)
                pv.SetSectionHighlight(true);
        }

        if (audio != null)
        {
            audio.Play();

            while (audio.isPlaying)
                yield return null;
        }

        foreach (PieceView pv in piezasSeccion)
        {
            if (pv != null)
                pv.SetSectionHighlight(false);
        }

        narracionActiva = false;
    }

    // Obtiene las piezas visuales que pertenecen a una misma sección del puzzle
    PieceView[] ObtenerPieceViewsPorTipo(SpecialPairType type)
    {
        System.Collections.Generic.List<PieceView> lista = new System.Collections.Generic.List<PieceView>();

        foreach (var p in model.pieces)
        {
            if (p.pairType == type)
            {
                if (piezasRoot == null)
                {
                    Debug.LogWarning("No se asignó piezasRoot en PuzzleController.");
                    continue;
                }

                Transform piezaTransform = piezasRoot.Find(p.id);

                if (piezaTransform == null)
                {
                    Debug.LogWarning("No encontró la pieza dentro de Piezas: " + p.id);
                    continue;
                }

                PieceView pv = piezaTransform.GetComponent<PieceView>();

                if (pv != null)
                    lista.Add(pv);
                else
                    Debug.LogWarning("No encontró PieceView en: " + p.id);
            }
        }

        return lista.ToArray();
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