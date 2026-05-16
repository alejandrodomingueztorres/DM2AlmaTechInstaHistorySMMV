// =============================================================================
// RetoRitualModel.cs
// Modelo del módulo "Reto Ritual". Gestiona todo el estado del juego:
//   - Fases y datos de cada fase (símbolo, video, desbloqueo)
//   - Lógica de los 3 minijuegos (sin dependencias de Unity UI)
//   - Eventos que notifican cambios a la Vista
// =============================================================================

using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

namespace RetoRitual
{
    // ─── Enumeraciones globales ───────────────────────────────────────────────

    public enum GamePhase
    {
        Phase1_Inhumacion = 0,
        Phase2_Ofrenda = 1,
        Phase3_Transito = 2,
        Completed = 3
    }

    public enum RitualState
    {
        PlayingMiniGame,    // El jugador está resolviendo el minijuego activo
        WatchingVideo,      // Se está reproduciendo un video de fase
        BrowsingIndicator,  // El jugador tiene el foco en el indicador de fases
        AllCompleted        // Todos los desafíos terminados
    }

    // ─── Datos de fase ────────────────────────────────────────────────────────

    [Serializable]
    public class PhaseData
    {
        public string name;
        public Sprite symbol;
        public VideoClip videoClip;
        [HideInInspector] public bool isUnlocked;
        [HideInInspector] public bool isCompleted;
    }

    // =========================================================================
    // MINIJUEGO 1 – INHUMACIÓN: Puzzle deslizante 3×3
    //   Inspiración RE: puzzle de lápida/símbolo partido en piezas.
    //   El jugador mueve las piezas con el D-Pad hasta recomponer
    //   el símbolo de la tumba de pozo.
    // =========================================================================
    [Serializable]
    public class SlidingPuzzleState
    {
        public const int SIZE = 9;
        public const int COLS = 3;

        // tiles[i]: valor 1-8 = pieza numerada, 0 = espacio vacío
        public int[] tiles = new int[SIZE];
        public int emptyIndex;
        public bool isComplete;

        // Estado resuelto: piezas 1-8 en orden, vacío al final
        private static readonly int[] SolvedTiles = { 0,1, 2, 3, 4, 5, 6, 7, 8};

        /// <summary>Establece un estado inicial mezclado (siempre resoluble).</summary>
        public void Shuffle()
        {
            // Configuración fija mezclada pero resoluble
            tiles = new int[] { 5, 3, 1, 8, 0, 2, 7, 4, 6 };
            emptyIndex = Array.IndexOf(tiles, 0);
            isComplete = false;
        }

        /// <summary>
        /// Intenta mover el espacio vacío en la dirección indicada
        /// (equivale a deslizar la pieza adyacente hacia el vacío).
        /// dir = Vector2Int.right → mueve vacío a la derecha (pieza de la derecha se desplaza).
        /// </summary>
        public bool TrySlide(Vector2Int dir)
        {
            int targetIndex = emptyIndex + dir.x + dir.y * COLS;

            // Fuera de los límites del array
            if (targetIndex < 0 || targetIndex >= SIZE) return false;

            // Evitar saltos de fila en movimientos horizontales
            if (dir.x < 0 && emptyIndex % COLS == 0) return false;
            if (dir.x > 0 && emptyIndex % COLS == COLS - 1) return false;

            // Intercambiar espacio vacío con la pieza destino
            (tiles[emptyIndex], tiles[targetIndex]) =
                (tiles[targetIndex], tiles[emptyIndex]);
            emptyIndex = targetIndex;

            // NO evaluamos isComplete aquí; lo hace el Modelo explícitamente
            // tras notificar a la Vista, para garantizar orden correcto.
            return true;
        }

        /// <summary>
        /// Verifica posición por posición si el puzzle está resuelto.
        /// Público para que el Modelo pueda llamarlo de forma explícita
        /// y controlada después de cada movimiento exitoso.
        /// </summary>
        public bool CheckSolved()
        {
            for (int i = 0; i < SIZE; i++)
                if (tiles[i] != SolvedTiles[i]) return false;
            isComplete = true;
            return true;
        }
    }

    // =========================================================================
    // MINIJUEGO 2 – OFRENDA: Correspondencia de símbolos (doble yo)
    //   Inspiración RE: puzzles de cuadros/estatuas que deben emparejarse.
    //   Dos columnas: izquierda = referencia fija (el "yo original"),
    //   derecha = ofrenda que el jugador reordena para reflejar la izquierda
    //   (el "doble yo").
    // =========================================================================
    [Serializable]
    public class OfrendaMatchState
    {
        public const int COUNT = 4;

        public int[] leftSymbols = new int[COUNT]; // referencia fija
        public int[] rightSymbols = new int[COUNT]; // el jugador las reordena
        public int cursorIndex;
        public int selectedIndex = -1;            // -1 = nada seleccionado
        public bool isComplete;

        public void Initialize()
        {
            leftSymbols = new int[] { 0, 1, 2, 3 };
            rightSymbols = new int[] { 2, 0, 3, 1 }; // mezclado
            cursorIndex = 0;
            selectedIndex = -1;
            isComplete = false;
        }

        /// <summary>Desplaza el cursor por la columna derecha (delta = +1 / -1).</summary>
        public void MoveCursor(int delta)
        {
            cursorIndex = Mathf.Clamp(cursorIndex + delta, 0, COUNT - 1);
        }

        /// <summary>
        /// Primera pulsación: selecciona la ranura bajo el cursor.
        /// Segunda pulsación: intercambia la selección con la ranura actual.
        /// </summary>
        public void Confirm()
        {
            if (selectedIndex < 0)
            {
                selectedIndex = cursorIndex;
            }
            else
            {
                (rightSymbols[selectedIndex], rightSymbols[cursorIndex]) =
                    (rightSymbols[cursorIndex], rightSymbols[selectedIndex]);
                selectedIndex = -1;
                isComplete = CheckSolved();
            }
        }

        /// <summary>Cancela la selección activa.</summary>
        public void Cancel() => selectedIndex = -1;

        private bool CheckSolved()
        {
            for (int i = 0; i < COUNT; i++)
                if (rightSymbols[i] != leftSymbols[i]) return false;
            return true;
        }
    }

    // =========================================================================
    // MINIJUEGO 3 – TRÁNSITO: Alineación de diales/anillos
    //   Inspiración RE: combinaciones de caja fuerte / medallones giratorios.
    //   Tres diales que representan los tres elementos del collar ritual
    //   (figura – collar – calavera). El jugador gira cada dial hasta
    //   alinearlos con los símbolos objetivo grabados en la estatua.
    // =========================================================================
    [Serializable]
    public class DialAlignmentState
    {
        public const int DIAL_COUNT = 3;
        public const int DIAL_STEPS = 6; // posiciones por dial (0-5)

        public int[] dialValues = new int[DIAL_COUNT];
        public int[] targetValues = new int[] { 0, 0, 0 }; // posición objetivo de cada dial
        public int activeDial;
        public bool isComplete;

        public void Initialize()
        {
            dialValues = new int[] { 5, 2, 3 };
            activeDial = 0;
            isComplete = false;
        }

        /// <summary>Gira el dial activo (delta = +1 / -1).</summary>
        public void RotateDial(int delta)
        {
            dialValues[activeDial] =
                (dialValues[activeDial] + delta + DIAL_STEPS) % DIAL_STEPS;
            isComplete = CheckSolved();
        }

        /// <summary>Cambia el dial activo (delta = +1 / -1).</summary>
        public void SwitchDial(int delta)
        {
            activeDial = (activeDial + delta + DIAL_COUNT) % DIAL_COUNT;
        }

        private bool CheckSolved()
        {
            for (int i = 0; i < DIAL_COUNT; i++)
                if (dialValues[i] != targetValues[i]) return false;
            return true;
        }
    }

    // =========================================================================
    // MODELO PRINCIPAL
    // =========================================================================
    public class RetoRitualModel : MonoBehaviour
    {
        // ─── Datos de fase (asignar en Inspector) ────────────────────────────
        [Header("Fases del Ritual")]
        [Tooltip("Índice 0 = Inhumación, 1 = Ofrenda, 2 = Tránsito")]
        public PhaseData[] phases = new PhaseData[3];

        // ─── Estado general ──────────────────────────────────────────────────
        public GamePhase CurrentPhase { get; private set; } = GamePhase.Phase1_Inhumacion;
        public RitualState RitualState { get; private set; } = RitualState.PlayingMiniGame;

        /// <summary>Índice del ícono enfocado en el indicador (0-2).</summary>
        public int IndicatorFocusIndex { get; private set; } = 0;

        // ─── Estados de minijuegos ───────────────────────────────────────────
        public SlidingPuzzleState MiniGame1 { get; } = new SlidingPuzzleState();
        public OfrendaMatchState MiniGame2 { get; } = new OfrendaMatchState();
        public DialAlignmentState MiniGame3 { get; } = new DialAlignmentState();

        // ─── Eventos → Vista ─────────────────────────────────────────────────
        public event Action<RitualState> OnRitualStateChanged;
        public event Action<GamePhase> OnPhaseChanged;
        public event Action OnAllPhasesCompleted;

        public event Action<SlidingPuzzleState> OnMiniGame1Updated;
        public event Action<OfrendaMatchState> OnMiniGame2Updated;
        public event Action<DialAlignmentState> OnMiniGame3Updated;

        public event Action<int> OnIndicatorFocusChanged;  // índice enfocado
        public event Action<int> OnPhaseVideoRequested;    // índice de fase

        private bool _isReplayingVideo = false;

        // =====================================================================
        // INICIALIZACIÓN
        // =====================================================================
        public void Initialize()
        {
            // Solo la fase 1 comienza desbloqueada
            phases[0].isUnlocked = true;
            phases[1].isUnlocked = false;
            phases[2].isUnlocked = false;

            CurrentPhase = GamePhase.Phase1_Inhumacion;
            SetRitualState(RitualState.PlayingMiniGame);

            MiniGame1.Shuffle();
            MiniGame2.Initialize();
            MiniGame3.Initialize();
        }

        // =====================================================================
        // MINIJUEGO 1 – INHUMACIÓN (Sliding Puzzle)
        // =====================================================================

        /// <summary>Desliza el espacio vacío en la dirección dada.</summary>
        public void Slide(Vector2Int direction)
        {
            if (!CanInteract(GamePhase.Phase1_Inhumacion)) return;
            if (!MiniGame1.TrySlide(direction)) return;

            // Verificar completado ANTES de notificar a la Vista,
            // así el flash de completado y la transición de fase ocurren en orden.
            bool solved = MiniGame1.CheckSolved();
            OnMiniGame1Updated?.Invoke(MiniGame1);

            Debug.Log(string.Join(",", MiniGame1.tiles));

            if (solved) CompleteCurrentPhase();
        }

        // =====================================================================
        // MINIJUEGO 2 – OFRENDA (Symbol Matching)
        // =====================================================================

        /// <summary>Mueve el cursor vertical en la columna derecha.</summary>
        public void MoveCursor2(int delta)
        {
            if (!CanInteract(GamePhase.Phase2_Ofrenda)) return;
            MiniGame2.MoveCursor(delta);
            OnMiniGame2Updated?.Invoke(MiniGame2);
        }

        /// <summary>Selecciona / intercambia símbolo bajo el cursor.</summary>
        public void Confirm2()
        {
            if (!CanInteract(GamePhase.Phase2_Ofrenda)) return;
            MiniGame2.Confirm();
            OnMiniGame2Updated?.Invoke(MiniGame2);
            if (MiniGame2.isComplete) CompleteCurrentPhase();
        }

        /// <summary>Cancela la selección activa.</summary>
        public void Cancel2()
        {
            if (!CanInteract(GamePhase.Phase2_Ofrenda)) return;
            MiniGame2.Cancel();
            OnMiniGame2Updated?.Invoke(MiniGame2);
        }

        // =====================================================================
        // MINIJUEGO 3 – TRÁNSITO (Dial Alignment)
        // =====================================================================

        /// <summary>Gira el dial activo (+1 / -1).</summary>
        public void RotateDial(int delta)
        {
            if (!CanInteract(GamePhase.Phase3_Transito)) return;
            MiniGame3.RotateDial(delta);
            OnMiniGame3Updated?.Invoke(MiniGame3);
            if (MiniGame3.isComplete) CompleteCurrentPhase();
        }

        /// <summary>Cambia el dial activo (+1 / -1).</summary>
        public void SwitchDial(int delta)
        {
            if (!CanInteract(GamePhase.Phase3_Transito)) return;
            MiniGame3.SwitchDial(delta);
            OnMiniGame3Updated?.Invoke(MiniGame3);
        }

        // =====================================================================
        // FLUJO DE FASES
        // =====================================================================

        /// <summary>Marca la fase actual como completada e inicia la reproducción de video.</summary>
        private void CompleteCurrentPhase()
        {
            int idx = (int)CurrentPhase;
            phases[idx].isCompleted = true;

            _isReplayingVideo = false; // bugfix

            SetRitualState(RitualState.WatchingVideo);
            OnPhaseVideoRequested?.Invoke(idx);
        }

        /// <summary>Llamado por la Vista cuando el video termina (o se salta).</summary>
        public void OnVideoFinished()
        {
            if (_isReplayingVideo)
            {
                _isReplayingVideo = false;

                SetRitualState(RitualState.PlayingMiniGame);

                OnPhaseChanged?.Invoke(CurrentPhase);

                return;
            }

            int nextIndex = (int)CurrentPhase + 1;

            if (nextIndex >= 3)
            {
                SetRitualState(RitualState.AllCompleted);
                OnAllPhasesCompleted?.Invoke();
                SceneManager.LoadScene("FuentesBibliograficas");
                return;
            }

            phases[nextIndex].isUnlocked = true;
            CurrentPhase = (GamePhase)nextIndex;

            SetRitualState(RitualState.PlayingMiniGame);
            OnPhaseChanged?.Invoke(CurrentPhase);
        }

        // =====================================================================
        // INDICADOR DE FASES
        // =====================================================================

        /// <summary>Activa o desactiva el foco sobre el indicador de fases.</summary>
        public void ToggleIndicatorFocus(bool focused)
        {
            if (RitualState == RitualState.WatchingVideo ||
                RitualState == RitualState.AllCompleted) return;

            SetRitualState(focused ? RitualState.BrowsingIndicator
                                   : RitualState.PlayingMiniGame);
        }

        /// <summary>Desplaza el foco dentro del indicador (+1 / -1).</summary>
        public void NavigateIndicator(int delta)
        {
            if (RitualState != RitualState.BrowsingIndicator) return;
            IndicatorFocusIndex = (IndicatorFocusIndex + delta + 3) % 3;
            OnIndicatorFocusChanged?.Invoke(IndicatorFocusIndex);
        }

        /// <summary>
        /// Solicita ver el video de la fase enfocada en el indicador,
        /// solo si ya ha sido completada.
        /// </summary>
        public void RequestVideoFromIndicator()
        {
            if (RitualState != RitualState.BrowsingIndicator) return;
            if (!phases[IndicatorFocusIndex].isCompleted) return;

            _isReplayingVideo = true; // bugfix

            SetRitualState(RitualState.WatchingVideo);
            OnPhaseVideoRequested?.Invoke(IndicatorFocusIndex);
        }

        // =====================================================================
        // HELPERS PRIVADOS
        // =====================================================================

        private bool CanInteract(GamePhase requiredPhase) =>
            RitualState == RitualState.PlayingMiniGame &&
            CurrentPhase == requiredPhase;

        private void SetRitualState(RitualState newState)
        {
            RitualState = newState;
            OnRitualStateChanged?.Invoke(newState);
        }

        public void SkipPhase1()
        {
            if (CurrentPhase != GamePhase.Phase1_Inhumacion) return;
            if (RitualState != RitualState.PlayingMiniGame) return;

            MiniGame1.isComplete = true;
            OnMiniGame1Updated?.Invoke(MiniGame1);  // Vista puede reaccionar si quiere

            CompleteCurrentPhase();  // → marca como completada → lanza video
        }
    }



}