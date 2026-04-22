using System.Collections;
using System.Collections.Generic;
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

    // Sistemas de partículas ubicados en el punto de unión de cada sección
    [Header("Partículas de unión")]
    public ParticleSystem baseSnapParticles;
    public ParticleSystem bodySnapParticles;
    public ParticleSystem neckSnapParticles;
    public ParticleSystem lipSnapParticles;
    public ParticleSystem symbolSnapParticles;
    public ParticleSystem dotsSnapParticles;

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

    // Posición pública del cursor para que otros sistemas (ej. AudioUIController) puedan usarla
    public Vector2 CursorPosition => cursorPosition;

    // Evento disparado cada vez que se presiona el botón de agarrar, incluso durante narraciones
    public event System.Action OnCursorClick;

    // Registra qué narraciones especiales ya se reprodujeron para no repetirlas
    private HashSet<SpecialPairType> narracionesReproducidas = new HashSet<SpecialPairType>();

    // AudioSource de la narración actualmente en reproducción (para pausar u omitir)
    private AudioSource narracionActualAudio;

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

        // El clic del cursor se propaga siempre, incluso durante narraciones,
        // para que los botones de UI (pausa, omitir) sigan respondiendo
        if (grabPressed)
            OnCursorClick?.Invoke();

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

                    // Si la pieza ya estaba colocada, se desmarca para permitir reposicionarla
                    if (selectedModel.isPlaced)
                    {
                        selectedModel.isPlaced = false;
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
            // Animación para que se deslice y rote suavemente al encajar
            StartCoroutine(AnimateSnap(selectedPiece.transform, selectedModel.targetTransform));

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

            // Partículas de unión al completar la sección
            PlaySnapParticlesByType(type);

            // Solo reproduce la narración si no se ha reproducido antes
            if (!narracionActiva && !narracionesReproducidas.Contains(type))
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

    // Pausa el audio de la narración activa sin cancelar la corrutina
    public void PausarNarracion()
    {
        if (narracionActualAudio != null && narracionActualAudio.isPlaying)
            narracionActualAudio.Pause();
    }

    // Reanuda el audio de la narración que fue pausada
    public void ReanudarNarracion()
    {
        if (narracionActualAudio != null && !narracionActualAudio.isPlaying)
            narracionActualAudio.UnPause();
    }

    // Detiene el audio activo; la corrutina detecta que dejó de reproducirse y finaliza sola
    public void OmitirNarracion()
    {
        if (narracionActualAudio != null)
            narracionActualAudio.Stop();
    }

    // Reproduce la narración de una sección y resalta visualmente sus piezas mientras dura el audio
    IEnumerator ReproducirNarracionConIluminacion(SpecialPairType type, AudioSource audio)
    {
        narracionActiva = true;
        narracionesReproducidas.Add(type); // Marca esta narración para no volver a reproducirla
        narracionActualAudio = audio;      // Expone el audio activo para pausa/omisión externa

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

        narracionActualAudio = null;
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

    // Corrutina encargada de animar el encaje de la pieza en su posición final
    IEnumerator AnimateSnap(Transform piece, Transform target)
    {
        Vector3 startPos = piece.position;
        Quaternion startRot = piece.rotation;

        Vector3 originalScale = piece.localScale;

        float duration = 0.12f; // duración del desplazamiento hacia el target
        float time = 0f;

        // Movimiento y rotación suave hacia el target
        while (time < duration)
        {
            piece.position = Vector3.Lerp(startPos, target.position, time / duration);
            piece.rotation = Quaternion.Slerp(startRot, target.rotation, time / duration);

            time += Time.deltaTime;
            yield return null;
        }

        // Ajusta la pieza exactamente en el punto final
        piece.position = target.position;
        piece.rotation = target.rotation;

        // Añade un pequeño rebote para reforzar el efecto de encaje
        piece.localScale = originalScale * 1.12f;
        yield return new WaitForSeconds(0.08f);
        piece.localScale = originalScale;
    }

        // Dispara las partículas en el punto de unión de la sección que se acaba de completar
        void PlaySnapParticlesByType(SpecialPairType type)
    {
        ParticleSystem ps = null;

        // Selecciona el sistema de partículas según la sección completada
        switch (type)
        {
            case SpecialPairType.Base:
                ps = baseSnapParticles;
                break;
            case SpecialPairType.Body:
                ps = bodySnapParticles;
                break;
            case SpecialPairType.Neck:
                ps = neckSnapParticles;
                break;
            case SpecialPairType.Lip:
                ps = lipSnapParticles;
                break;
            case SpecialPairType.Symbol:
                ps = symbolSnapParticles;
                break;
            case SpecialPairType.Dots:
                ps = dotsSnapParticles;
                break;
        }

        // Reproduce las partículas si existe una referencia asignada
        if (ps != null)
            ps.Play();
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