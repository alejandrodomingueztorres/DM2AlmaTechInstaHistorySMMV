// =============================================================================
// RetoRitualController.cs
// Controlador del módulo "Reto Ritual". Responsabilidades:
//   - Registrar y gestionar los InputActions del New Input System de Unity
//     (control de Xbox: D-Pad/Stick, A, B, LB, RB, Y)
//   - Traducir inputs en llamadas al Modelo
//   - Orquestar el inicio de la experiencia (inicializa Modelo y Vista)
//   - Gestionar el hold del botón B para saltar videos
//   - Aplicar cooldown de input para evitar disparos múltiples
//
// Configuración en el Inspector:
//   1. Asignar las InputActionReference de tu InputActionAsset.
//   2. Las referencias de Modelo y Vista se obtienen automáticamente
//      del mismo GameObject (RequireComponent).
//
// Mapeo de controles por estado:
//   ┌────────────────────┬─────────────────────────────────────────────────┐
//   │ Estado             │ Input                                           │
//   ├────────────────────┼─────────────────────────────────────────────────┤
//   │ PlayingMiniGame    │ D-Pad/Stick → acción según fase activa          │
//   │ (Fase 1)           │   ← → ↑ ↓ : deslizar pieza del puzzle          │
//   │ (Fase 2)           │   ↑ ↓ : mover cursor  │  A: selec/intercambiar │
//   │                    │   B: cancelar selección                         │
//   │ (Fase 3)           │   ↑ ↓ : girar dial activo                      │
//   │                    │   LB / ← : dial anterior                       │
//   │                    │   RB / → : dial siguiente                      │
//   ├────────────────────┼─────────────────────────────────────────────────┤
//   │ WatchingVideo      │   B (hold 1.5 s) : saltar video                 │
//   ├────────────────────┼─────────────────────────────────────────────────┤
//   │ BrowsingIndicator  │   ← → / LB / RB : navegar íconos               │
//   │                    │   A : ver video de fase (si está desbloqueada)  │
//   │                    │   B / Y : cerrar indicador                      │
//   ├────────────────────┼─────────────────────────────────────────────────┤
//   │ Cualquier estado   │   Y : abrir/cerrar indicador de fases           │
//   └────────────────────┴─────────────────────────────────────────────────┘
// =============================================================================

using UnityEngine;
using UnityEngine.InputSystem;

namespace RetoRitual
{
    [RequireComponent(typeof(RetoRitualModel))]
    [RequireComponent(typeof(RetoRitualView))]
    public class RetoRitualController : MonoBehaviour
    {
        // ─── Referencias MVC ──────────────────────────────────────────────────
        private RetoRitualModel _model;
        private RetoRitualView  _view;

        // =====================================================================
        // INPUT ACTION REFERENCES (asignar en Inspector)
        // Crear un InputActionAsset con las siguientes acciones y hacer binding
        // con el control de Xbox:
        //   Move       → <Gamepad>/dpad  +  <Gamepad>/leftStick
        //   Confirm    → <Gamepad>/buttonSouth      (A)
        //   Cancel     → <Gamepad>/buttonEast       (B)
        //   LeftBumper → <Gamepad>/leftShoulder     (LB)
        //   RightBumper→ <Gamepad>/rightShoulder    (RB)
        //   Indicator  → <Gamepad>/buttonNorth      (Y)
        // =====================================================================
        [Header("─── Input Action References ──────────────────────")]
        [Tooltip("D-Pad + Stick izquierdo → Vector2")]
        [SerializeField] private InputActionReference moveAction;
        [Tooltip("Botón A")]
        [SerializeField] private InputActionReference confirmAction;
        [Tooltip("Botón B (pulsación breve = cancelar; hold = saltar video)")]
        [SerializeField] private InputActionReference cancelAction;
        [Tooltip("LB – bumper izquierdo")]
        [SerializeField] private InputActionReference leftBumperAction;
        [Tooltip("RB – bumper derecho")]
        [SerializeField] private InputActionReference rightBumperAction;
        [Tooltip("Botón Y – toggle indicador de fases")]
        [SerializeField] private InputActionReference indicatorToggleAction;

        // ─── Parámetros de input ──────────────────────────────────────────────
        [Header("─── Parámetros de Input ────────────────────────")]
        [Tooltip("Tiempo mínimo entre inputs repetidos (segundos)")]
        [SerializeField] private float inputCooldown       = 0.18f;
        [Tooltip("Tiempo de hold del botón B para saltar un video (segundos)")]
        [SerializeField] private float videoSkipHoldTime   = 1.5f;
        [Tooltip("Umbral de magnitud del stick para registrar dirección")]
        [SerializeField] private float stickDeadzone       = 0.5f;

        // ─── Estado interno ───────────────────────────────────────────────────
        private float _lastInputTime  = 0f;
        private float _cancelHoldStart = -1f;   // -1 = botón B no está pulsado

        private bool IsInputReady =>
            Time.unscaledTime - _lastInputTime >= inputCooldown;

        // =====================================================================
        // INICIALIZACIÓN
        // =====================================================================

        private void Awake()
        {
            _model = GetComponent<RetoRitualModel>();
            _view  = GetComponent<RetoRitualView>();
        }

        private void Start()
        {
            // El Controlador arranca el ciclo MVC
            _model.Initialize();
            _view.Initialize(_model);
        }

        // =====================================================================
        // SUSCRIPCIÓN / DESUSCRIPCIÓN DE INPUTS
        // =====================================================================

        private void OnEnable()
        {
            RegisterActions(true);
            EnableActions(true);
        }

        private void OnDisable()
        {
            RegisterActions(false);
            EnableActions(false);
        }

        private void RegisterActions(bool register)
        {
            if (moveAction != null)
            {
                if (register) moveAction.action.performed        += OnMove;
                else          moveAction.action.performed        -= OnMove;
            }
            if (confirmAction != null)
            {
                if (register) confirmAction.action.performed     += OnConfirm;
                else          confirmAction.action.performed     -= OnConfirm;
            }
            if (cancelAction != null)
            {
                if (register)
                {
                    cancelAction.action.started  += OnCancelStarted;
                    cancelAction.action.canceled += OnCancelReleased;
                }
                else
                {
                    cancelAction.action.started  -= OnCancelStarted;
                    cancelAction.action.canceled -= OnCancelReleased;
                }
            }
            if (leftBumperAction != null)
            {
                if (register) leftBumperAction.action.performed  += OnLeftBumper;
                else          leftBumperAction.action.performed  -= OnLeftBumper;
            }
            if (rightBumperAction != null)
            {
                if (register) rightBumperAction.action.performed += OnRightBumper;
                else          rightBumperAction.action.performed -= OnRightBumper;
            }
            if (indicatorToggleAction != null)
            {
                if (register) indicatorToggleAction.action.performed += OnIndicatorToggle;
                else          indicatorToggleAction.action.performed -= OnIndicatorToggle;
            }
        }

        private void EnableActions(bool enable)
        {
            if (enable)
            {
                moveAction?.action.Enable();
                confirmAction?.action.Enable();
                cancelAction?.action.Enable();
                leftBumperAction?.action.Enable();
                rightBumperAction?.action.Enable();
                indicatorToggleAction?.action.Enable();
            }
            else
            {
                moveAction?.action.Disable();
                confirmAction?.action.Disable();
                cancelAction?.action.Disable();
                leftBumperAction?.action.Disable();
                rightBumperAction?.action.Disable();
                indicatorToggleAction?.action.Disable();
            }
        }

        // =====================================================================
        // UPDATE – HOLD DEL BOTÓN B PARA SALTAR VIDEO
        // =====================================================================

        private void Update()
        {
            HandleCancelHold();
        }

        private void HandleCancelHold()
        {
            if (_cancelHoldStart < 0f) return;
            if (_model.RitualState != RitualState.WatchingVideo) return;

            float held = Time.unscaledTime - _cancelHoldStart;
            if (held >= videoSkipHoldTime)
            {
                _cancelHoldStart = -1f;
                _view.SkipVideo();
            }
        }

        // =====================================================================
        // CALLBACKS DE INPUT
        // =====================================================================

        // ─── D-Pad / Stick izquierdo ──────────────────────────────────────────
        private void OnMove(InputAction.CallbackContext ctx)
        {
            if (!IsInputReady) return;
            _lastInputTime = Time.unscaledTime;

            Vector2    raw = ctx.ReadValue<Vector2>();
            Vector2Int dir = GetCardinalDirection(raw);
            if (dir == Vector2Int.zero) return;

            switch (_model.RitualState)
            {
                case RitualState.PlayingMiniGame:
                    DispatchMoveToMiniGame(dir);
                    break;

                case RitualState.BrowsingIndicator:
                    // Solo movimiento horizontal en el indicador
                    if (dir.x != 0) _model.NavigateIndicator(dir.x);
                    break;

                // WatchingVideo / AllCompleted: movimiento ignorado
            }
        }

        /// <summary>Enruta el input de movimiento al minijuego correspondiente.</summary>
        private void DispatchMoveToMiniGame(Vector2Int dir)
        {
            switch (_model.CurrentPhase)
            {
                // ── Fase 1: Sliding Puzzle ──────────────────────────────────
                // El jugador empuja una pieza en la dirección del joystick,
                // por lo que el espacio vacío se mueve en sentido contrario
                // horizontalmente. El eje Y no necesita inversión porque
                // en UI el eje Y positivo apunta hacia arriba en el input
                // pero hacia abajo en el índice del array (fila siguiente).
                case GamePhase.Phase1_Inhumacion:
                    _model.Slide(new Vector2Int(dir.x, -dir.y));
                    break;

                // ── Fase 2: Symbol Matching ─────────────────────────────────
                // ↑ / ↓  →  navegar ranuras de la columna derecha.
                // (A para seleccionar/intercambiar, B para cancelar selección)
                case GamePhase.Phase2_Ofrenda:
                    if (dir.y != 0)
                        _model.MoveCursor2(-dir.y); // Y invertido: ↓ = índice +1
                    break;

                // ── Fase 3: Dial Alignment ──────────────────────────────────
                // ↑ / ↓  →  girar dial activo.
                // ← / →  →  cambiar dial activo (duplicado con LB/RB).
                case GamePhase.Phase3_Transito:
                    if (dir.y != 0) _model.RotateDial(dir.y);
                    if (dir.x != 0) _model.SwitchDial(dir.x);
                    break;
            }
        }

        // ─── Botón A – Confirmar ──────────────────────────────────────────────
        private void OnConfirm(InputAction.CallbackContext ctx)
        {
            if (!IsInputReady) return;
            _lastInputTime = Time.unscaledTime;

            switch (_model.RitualState)
            {
                case RitualState.PlayingMiniGame:
                    // Solo Fase 2 requiere confirmación explícita (selec/swap)
                    if (_model.CurrentPhase == GamePhase.Phase2_Ofrenda)
                        _model.Confirm2();
                    break;

                case RitualState.BrowsingIndicator:
                    // Solicitar video de la fase enfocada (si está completada)
                    _model.RequestVideoFromIndicator();
                    break;
            }
        }

        // ─── Botón B – Cancelar / Hold para saltar video ─────────────────────
        private void OnCancelStarted(InputAction.CallbackContext ctx)
        {
            _cancelHoldStart = Time.unscaledTime;

            // Acciones inmediatas en la pulsación (sin esperar hold)
            switch (_model.RitualState)
            {
                case RitualState.PlayingMiniGame:
                    if (_model.CurrentPhase == GamePhase.Phase2_Ofrenda)
                        _model.Cancel2(); // deseleccionar símbolo activo
                    break;

                case RitualState.BrowsingIndicator:
                    _model.ToggleIndicatorFocus(false); // cerrar indicador
                    break;

                // WatchingVideo: el hold se gestiona en Update()
            }
        }

        private void OnCancelReleased(InputAction.CallbackContext ctx)
        {
            _cancelHoldStart = -1f;
        }

        // ─── LB – Bumper izquierdo ────────────────────────────────────────────
        private void OnLeftBumper(InputAction.CallbackContext ctx)
        {
            if (!IsInputReady) return;
            _lastInputTime = Time.unscaledTime;

            switch (_model.RitualState)
            {
                case RitualState.PlayingMiniGame:
                    if (_model.CurrentPhase == GamePhase.Phase3_Transito)
                        _model.SwitchDial(-1); // dial anterior
                    break;

                case RitualState.BrowsingIndicator:
                    _model.NavigateIndicator(-1);
                    break;
            }
        }

        // ─── RB – Bumper derecho ──────────────────────────────────────────────
        private void OnRightBumper(InputAction.CallbackContext ctx)
        {
            if (!IsInputReady) return;
            _lastInputTime = Time.unscaledTime;

            switch (_model.RitualState)
            {
                case RitualState.PlayingMiniGame:
                    if (_model.CurrentPhase == GamePhase.Phase3_Transito)
                        _model.SwitchDial(1); // dial siguiente
                    break;

                case RitualState.BrowsingIndicator:
                    _model.NavigateIndicator(1);
                    break;
            }
        }

        // ─── Botón Y – Toggle indicador de fases ─────────────────────────────
        private void OnIndicatorToggle(InputAction.CallbackContext ctx)
        {
            if (!IsInputReady) return;
            _lastInputTime = Time.unscaledTime;

            // No permitir abrir el indicador mientras se ve un video
            if (_model.RitualState == RitualState.WatchingVideo ||
                _model.RitualState == RitualState.AllCompleted) return;

            bool isBrowsing = _model.RitualState == RitualState.BrowsingIndicator;
            _model.ToggleIndicatorFocus(!isBrowsing);
        }

        // =====================================================================
        // HELPERS
        // =====================================================================

        /// <summary>
        /// Convierte un Vector2 analógico en una de las 4 direcciones cardinales.
        /// Devuelve Vector2Int.zero si la magnitud no supera el deadzone.
        /// </summary>
        private Vector2Int GetCardinalDirection(Vector2 input)
        {
            if (input.magnitude < stickDeadzone) return Vector2Int.zero;

            // Proyectar al eje dominante
            return Mathf.Abs(input.x) >= Mathf.Abs(input.y)
                ? (input.x > 0 ? Vector2Int.right : Vector2Int.left)
                : (input.y > 0 ? Vector2Int.up    : Vector2Int.down);
        }
    }
}