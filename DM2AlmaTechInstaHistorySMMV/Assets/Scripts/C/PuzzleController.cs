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

    public Transform piezasRoot;

    [Header("Narraciones")]
    public AudioSource audioBase;
    public AudioSource audioCuerpo;
    public AudioSource audioCuello;
    public AudioSource audioBorde;
    public AudioSource audioSimbolo;
    public AudioSource audioPuntos;

    private bool narracionActiva = false;

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

    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool grabPressed;
    private bool releasePressed;

    private GameObject selectedPiece;
    private PieceModel selectedModel;
    private PieceView selectedView;
    private InputSystem_Actions inputActions;

    private Vector3 currentPiecePosition;
    private Vector2 cursorPosition;

    public Vector2 CursorPosition => cursorPosition;
    public event System.Action OnCursorClick;

    private HashSet<SpecialPairType> narracionesReproducidas = new HashSet<SpecialPairType>();
    private HashSet<SpecialPairType> metricasParesRegistradas = new HashSet<SpecialPairType>();

    private AudioSource narracionActualAudio;
    private Texture2D whiteTexture;

    public bool boxMode;

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

        Debug.Log("[PuzzleController] Puzzle iniciado correctamente.");

        if (MetricsManager.Instance == null)
            Debug.LogWarning("[PuzzleController] No se encontró MetricsManager en la escena.");
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

        inputActions.Puzzle.skip.performed += ctx => OmitirNarracion();
    }

    void OnDisable()
    {
        inputActions.Puzzle.Disable();

        if (MetricsManager.Instance != null)
        {
            MetricsManager.Instance.EndSession();
            Debug.Log("[PuzzleController] Sesión de métricas cerrada desde OnDisable.");
        }
    }

    void Update()
    {
        UpdateCursor();

        if (grabPressed)
        {
            OnCursorClick?.Invoke();
            RegisterMetricInteraction("Click de cursor / intento de selección");
        }

        if (!narracionActiva)
        {
            HandleSelection();
            HandleMovement();
        }

        grabPressed = false;
        releasePressed = false;
    }

    void UpdateCursor()
    {

        if (boxMode == true)
        {
            cursorPosition.x -= lookInput.x * cursorSpeed * Time.deltaTime;
            cursorPosition.y += lookInput.y * cursorSpeed * Time.deltaTime;
        }
        else
        {
            cursorPosition += lookInput * cursorSpeed * Time.deltaTime;
        }
        cursorPosition.x = Mathf.Clamp(cursorPosition.x, 0, Screen.width);
        cursorPosition.y = Mathf.Clamp(cursorPosition.y, 0, Screen.height);
    }

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

                    if (selectedModel.isPlaced)
                    {
                        selectedModel.isPlaced = false;
                    }

                    currentPiecePosition = selectedPiece.transform.position;

                    Debug.Log("[PuzzleController] Pieza seleccionada: " + selectedPiece.name);
                    RegisterMetricInteraction("Pieza seleccionada: " + selectedPiece.name);
                }
            }
        }

        if (releasePressed && selectedPiece != null)
        {
            Debug.Log("[PuzzleController] Pieza soltada: " + selectedPiece.name);
            RegisterMetricInteraction("Pieza soltada: " + selectedPiece.name);

            TryPlacePiece();

            selectedPiece = null;
            selectedModel = null;
            selectedView = null;
        }
    }

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
            StartCoroutine(AnimateSnap(selectedPiece.transform, selectedModel.targetTransform));

            selectedModel.isPlaced = true;
            selectedView.PlayCorrectFeedback();

            Debug.Log("[PuzzleController] Pieza colocada correctamente: " + selectedPiece.name);
            RegisterMetricInteraction("Pieza colocada correctamente: " + selectedPiece.name);

            CheckSpecialPairs(selectedModel.pairType);

            if (model.AreAllPlaced())
            {
                OnAllPiecesPlaced();
            }
        }
        else
        {
            selectedView.PlayWrongFeedback();

            Debug.Log("[PuzzleController] Intento incorrecto con pieza: " + selectedPiece.name);
            RegisterMetricInteraction("Intento incorrecto: " + selectedPiece.name);
        }
    }

    void CheckSpecialPairs(SpecialPairType type)
    {
        if (type == SpecialPairType.None) return;

        if (model.CheckSpecialPair(type))
        {
            Debug.Log("Par especial completado: " + type);

            if (!metricasParesRegistradas.Contains(type))
            {
                metricasParesRegistradas.Add(type);
                RegisterMetricStage("Par especial completado: " + type);
            }

            PlaySnapParticlesByType(type);

            if (!narracionActiva && !narracionesReproducidas.Contains(type))
            {
                if (type == SpecialPairType.Symbol)
                    StartCoroutine(ReproducirNarracionConIluminacion(type, audioSimbolo));
                else if (type == SpecialPairType.Dots)
                    StartCoroutine(ReproducirNarracionConIluminacion(type, audioPuntos));
                else if (type == SpecialPairType.Base)
                    StartCoroutine(ReproducirNarracionConIluminacion(type, audioBase));
                else if (type == SpecialPairType.Body)
                    StartCoroutine(ReproducirNarracionConIluminacion(type, audioCuerpo));
                else if (type == SpecialPairType.Neck)
                    StartCoroutine(ReproducirNarracionConIluminacion(type, audioCuello));
                else if (type == SpecialPairType.Lip)
                    StartCoroutine(ReproducirNarracionConIluminacion(type, audioBorde));
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
        Debug.Log("Puzzle completado!");

        RegisterMetricStage("Puzzle completado");

        if (MetricsManager.Instance != null)
        {
            MetricsManager.Instance.EndSession();

            Debug.Log("========== MÉTRICAS DEL PUZZLE ==========");
            Debug.Log("Visitantes acumulados: " + MetricsManager.Instance.TotalVisitors);
            Debug.Log("Duración acumulada: " + MetricsManager.Instance.TotalSessionDuration.ToString("F2") + " segundos");
            Debug.Log("Interacciones acumuladas: " + MetricsManager.Instance.TotalInteractions);
            Debug.Log("Etapas completadas acumuladas: " + MetricsManager.Instance.CompletedStages);
            Debug.Log("=========================================");
        }
    }

    public void PausarNarracion()
    {
        if (narracionActualAudio != null && narracionActualAudio.isPlaying)
            narracionActualAudio.Pause();
    }

    public void ReanudarNarracion()
    {
        if (narracionActualAudio != null && !narracionActualAudio.isPlaying)
            narracionActualAudio.UnPause();
    }

    public void OmitirNarracion()
    {
        if (narracionActualAudio != null)
            narracionActualAudio.Stop();
    }

    IEnumerator ReproducirNarracionConIluminacion(SpecialPairType type, AudioSource audio)
    {
        narracionActiva = true;
        narracionesReproducidas.Add(type);
        narracionActualAudio = audio;

        PieceView[] piezasSeccion = ObtenerPieceViewsPorTipo(type);

        foreach (PieceView pv in piezasSeccion)
        {
            if (pv != null)
                pv.SetSectionHighlight(true);
        }

        if (audio != null)
        {
            Debug.Log("[PuzzleController] Reproduciendo narración de sección: " + type);
            RegisterMetricInteraction("Narración reproducida: " + type);

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

    PieceView[] ObtenerPieceViewsPorTipo(SpecialPairType type)
    {
        List<PieceView> lista = new List<PieceView>();

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

    IEnumerator AnimateSnap(Transform piece, Transform target)
    {
        Vector3 startPos = piece.position;
        Quaternion startRot = piece.rotation;
        Vector3 originalScale = piece.localScale;

        float duration = 0.12f;
        float time = 0f;

        while (time < duration)
        {
            piece.position = Vector3.Lerp(startPos, target.position, time / duration);
            piece.rotation = Quaternion.Slerp(startRot, target.rotation, time / duration);

            time += Time.deltaTime;
            yield return null;
        }

        piece.position = target.position;
        piece.rotation = target.rotation;

        piece.localScale = originalScale * 1.12f;
        yield return new WaitForSeconds(0.08f);
        piece.localScale = originalScale;
    }

    void PlaySnapParticlesByType(SpecialPairType type)
    {
        ParticleSystem ps = null;

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

        if (ps != null)
            ps.Play();
    }

    private void RegisterMetricInteraction(string description)
    {
        if (MetricsManager.Instance != null)
            MetricsManager.Instance.RegisterInteraction(description);
        else
            Debug.LogWarning("[Metrics] No se registró interacción porque no existe MetricsManager.");
    }

    private void RegisterMetricStage(string description)
    {
        if (MetricsManager.Instance != null)
            MetricsManager.Instance.CompleteStage(description);
        else
            Debug.LogWarning("[Metrics] No se registró etapa porque no existe MetricsManager.");
    }

    void OnGUI()
    {
        float size = 14f;
        float thickness = 2f;

        Color previous = GUI.color;
        GUI.color = Color.green;

        GUI.DrawTexture(new Rect(cursorPosition.x - size / 2, Screen.height - cursorPosition.y, size, thickness), whiteTexture);
        GUI.DrawTexture(new Rect(cursorPosition.x, Screen.height - cursorPosition.y - size / 2, thickness, size), whiteTexture);

        GUI.color = previous;
    }
}