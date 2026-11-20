// =============================================================================
// RetoRitualView.cs
// Vista del módulo "Reto Ritual". Responsabilidades:
//   - Suscribirse a los eventos del Modelo y reflejar cambios en la UI
//   - Renderizar los 3 minijuegos (sliding puzzle, matching, diales)
//   - Gestionar el indicador de fases
//   - Controlar el VideoPlayer (reproducir / saltar videos)
//   - Efectos visuales de feedback (selección, completado)
// NOTA: La Vista NO modifica el estado del Modelo. Solo lee datos
//       a través de eventos y propiedades públicas del Modelo.
// =============================================================================

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

namespace RetoRitual
{
    public class RetoRitualView : MonoBehaviour
    {
        private RetoRitualModel _model;

        // Estado visual del reto 1
        private Coroutine _sequencePlayback;
        private bool _mg1CompletionShown;

        // =====================================================================
        // REFERENCIAS DE UI (asignar en Inspector)
        // =====================================================================

        [Header("─── Paneles principales ───────────────────────")]
        [SerializeField] private GameObject miniGame1Panel;
        [SerializeField] private GameObject miniGame2Panel;
        [SerializeField] private GameObject miniGame3Panel;
        [SerializeField] private GameObject videoPanel;
        [SerializeField] private GameObject completionPanel;

        // ─── MiniGame 1: Los Cuatro Rumbos – Secuencia ────────────────────────
        [Header("─── MG1 Inhumación – Los Cuatro Rumbos ────────")]
        [Tooltip("4 piedras/símbolos en este orden: 0 = Arriba, 1 = Derecha, 2 = Abajo, 3 = Izquierda. " +
                 "Usar sprites claros/blancos: los colores de abajo se aplican como tinte.")]
        [SerializeField] private Image[] sequenceStoneImages;
        [Tooltip("(Opcional) Puntos de progreso. Solo se muestran tantos como pasos tenga la secuencia")]
        [SerializeField] private Image[] sequenceProgressPips;
        [Tooltip("(Opcional) Texto de ayuda del reto")]
        [SerializeField] private TextMeshProUGUI sequenceHintText;

        [SerializeField] private Color stoneIdleColor = new Color(0.55f, 0.55f, 0.55f);
        [SerializeField] private Color stoneLitColor = new Color(1f, 0.85f, 0.1f);
        [SerializeField] private Color stoneCorrectColor = new Color(0.3f, 1f, 0.5f);
        [SerializeField] private Color stoneWrongColor = new Color(1f, 0.25f, 0.2f);
        [SerializeField] private Color pipEmptyColor = new Color(0.25f, 0.25f, 0.25f);
        [SerializeField] private Color pipFilledColor = new Color(0.3f, 1f, 0.5f);

        [Header("MG1 – Tiempos (segundos)")]
        [Tooltip("Pausa antes de mostrar la secuencia por primera vez / al pulsar A")]
        [SerializeField] private float playbackStartDelay = 0.6f;
        [Tooltip("Tiempo que cada piedra permanece iluminada")]
        [SerializeField] private float stepLitTime = 0.45f;
        [Tooltip("Pausa entre piedras")]
        [SerializeField] private float stepGapTime = 0.2f;
        [Tooltip("Pausa tras un error antes de repetir la secuencia")]
        [SerializeField] private float mistakeReplayDelay = 0.8f;
        [SerializeField] private float pressFlashTime = 0.15f;
        [SerializeField] private float wrongFlashTime = 0.5f;
        [Tooltip("Escala de la piedra iluminada (1 = sin efecto)")]
        [SerializeField] private float litScale = 1.15f;

        // ─── MiniGame 2: Ofrenda – Symbol Matching ───────────────────────────
        [Header("─── MG2 Ofrenda – Symbol Matching ─────────────")]
        [Tooltip("4 ranuras de referencia (columna izquierda, fijas)")]
        [SerializeField] private Image[] leftOfrendaSlots;
        [Tooltip("4 ranuras que el jugador reordena (columna derecha)")]
        [SerializeField] private Image[] rightOfrendaSlots;
        [Tooltip("Sprites de los 4 símbolos de ofrenda")]
        [SerializeField] private Sprite[] ofrendaSymbols;
        [SerializeField] private Color normalSlotColor = Color.white;
        [SerializeField] private Color cursorSlotColor = new Color(1f, 0.9f, 0.2f);
        [SerializeField] private Color selectedSlotColor = new Color(0.2f, 0.9f, 1f);
        [SerializeField] private Color matchedSlotColor = new Color(0.2f, 1f, 0.4f);

        // ─── MiniGame 3: Tránsito – Dial Alignment ───────────────────────────
        [Header("─── MG3 Tránsito – Dial Alignment ──────────────")]
        [Tooltip("3 imágenes de cara de dial (una por dial)")]
        [SerializeField] private Image[] dialImages;
        [Tooltip("6 sprites por paso de dial. Total = 6 sprites")]
        [SerializeField] private Sprite[] dialStepSprites;
        [Tooltip("Bordes/marcos de cada dial para indicar cuál está activo")]
        [SerializeField] private Image[] dialFrames;
        [SerializeField] private Color activeDialColor = new Color(1f, 0.85f, 0.1f);
        [SerializeField] private Color inactiveDialColor = new Color(0.5f, 0.5f, 0.5f);
        [Tooltip("Indicadores opcionales del símbolo objetivo de cada dial")]
        [SerializeField] private Image[] dialTargetImages;

        // ─── Video Player ─────────────────────────────────────────────────────
        [Header("─── Video Player ──────────────────────────────────")]
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private RawImage videoRawImage;
        [SerializeField] private RenderTexture videoRenderTexture;
        [Tooltip("Texto de ayuda visible durante la reproducción (e.g. 'Mantén B para saltar')")]
        [SerializeField] private TextMeshProUGUI skipHintText;
        [SerializeField] private Slider videoProgressBar;

        // ─── Indicador de fases ───────────────────────────────────────────────
        [Header("─── Indicador de Fases ─────────────────────────")]
        [Tooltip("3 imágenes de símbolo por fase (orden: Inhumación, Ofrenda, Tránsito)")]
        [SerializeField] private Image[] phaseSymbolImages;
        [Tooltip("3 marcos de ícono (para recolorear según estado)")]
        [SerializeField] private Image[] phaseSymbolFrames;
        [Tooltip("Highlight que aparece sobre el ícono enfocado en modo indicador")]
        [SerializeField] private GameObject indicatorFocusCursor;
        [Tooltip("Nombre de la fase enfocada (opcional)")]
        [SerializeField] private TextMeshProUGUI phaseNameLabel;
        [Tooltip("Texto de ayuda del indicador (e.g. 'A: ver video  B: cerrar')")]
        [SerializeField] private TextMeshProUGUI indicatorHintText;

        [SerializeField] private Color lockedColor = new Color(0.25f, 0.25f, 0.25f);
        [SerializeField] private Color unlockedColor = Color.white;
        [SerializeField] private Color completedColor = new Color(0.3f, 1f, 0.5f);
        [SerializeField] private Color focusedColor = new Color(1f, 0.85f, 0.1f);

        // =====================================================================
        // INICIALIZACIÓN
        // =====================================================================

        /// <summary>
        /// Llamado por el Controlador. Se suscribe al Modelo y
        /// establece el estado visual inicial.
        /// </summary>
        public void Initialize(RetoRitualModel model)
        {
            _model = model;
            SubscribeToModelEvents();

            // Sprites de fases en el indicador
            for (int i = 0; i < 3; i++)
                if (_model.phases[i].symbol != null)
                    phaseSymbolImages[i].sprite = _model.phases[i].symbol;

            // Columna izquierda de Ofrenda: referencia fija
            for (int i = 0; i < OfrendaMatchState.COUNT; i++)
                leftOfrendaSlots[i].sprite = ofrendaSymbols[i];

            // Mostrar sprites objetivo del dial (si se asignaron)
            if (dialTargetImages != null)
            {
                for (int i = 0; i < DialAlignmentState.DIAL_COUNT; i++)
                {
                    int target = _model.MiniGame3.targetValues[i];
                    if (i < dialTargetImages.Length && dialTargetImages[i] != null)
                        dialTargetImages[i].sprite = dialStepSprites[target];
                }
            }

            // Configurar VideoPlayer
            if (videoPlayer != null && videoRenderTexture != null)
            {
                videoPlayer.targetTexture = videoRenderTexture;
                if (videoRawImage != null)
                    videoRawImage.texture = videoRenderTexture;
            }

            // Estado inicial: panel fase 1, resto ocultos
            ShowGamePanel(GamePhase.Phase1_Inhumacion);
            ResetSequenceStones();
            RefreshMiniGame1(_model.MiniGame1);
            RefreshPhaseIndicator();
        }

        private void SubscribeToModelEvents()
        {
            _model.OnRitualStateChanged += HandleRitualStateChanged;
            _model.OnPhaseChanged += HandlePhaseChanged;
            _model.OnMiniGame1Updated += RefreshMiniGame1;
            _model.OnSequencePlaybackRequested += HandleSequencePlaybackRequested;
            _model.OnSequenceInput += HandleSequenceInput;
            _model.OnMiniGame2Updated += RefreshMiniGame2;
            _model.OnMiniGame3Updated += RefreshMiniGame3;
            _model.OnIndicatorFocusChanged += HandleIndicatorFocusChanged;
            _model.OnPhaseVideoRequested += PlayPhaseVideo;
            _model.OnAllPhasesCompleted += ShowCompletion;
        }

        private void OnDestroy()
        {
            if (_model == null) return;
            _model.OnRitualStateChanged -= HandleRitualStateChanged;
            _model.OnPhaseChanged -= HandlePhaseChanged;
            _model.OnMiniGame1Updated -= RefreshMiniGame1;
            _model.OnSequencePlaybackRequested -= HandleSequencePlaybackRequested;
            _model.OnSequenceInput -= HandleSequenceInput;
            _model.OnMiniGame2Updated -= RefreshMiniGame2;
            _model.OnMiniGame3Updated -= RefreshMiniGame3;
            _model.OnIndicatorFocusChanged -= HandleIndicatorFocusChanged;
            _model.OnPhaseVideoRequested -= PlayPhaseVideo;
            _model.OnAllPhasesCompleted -= ShowCompletion;
        }

        // =====================================================================
        // MANEJADORES DE EVENTOS DEL MODELO
        // =====================================================================

        private void HandleRitualStateChanged(RitualState state)
        {
            bool isBrowsing = state == RitualState.BrowsingIndicator;

            if (indicatorFocusCursor != null)
                indicatorFocusCursor.SetActive(isBrowsing);

            if (indicatorHintText != null)
                indicatorHintText.text = isBrowsing
                    ? "◄ ► Navegar   A Ver video   B Cerrar"
                    : "Y Indicador de fases";

            RefreshPhaseIndicator();
        }

        private void HandlePhaseChanged(GamePhase phase)
        {
            ShowGamePanel(phase);
            RefreshPhaseIndicator();

            // Inicializar la vista del nuevo minijuego
            switch (phase)
            {
                case GamePhase.Phase2_Ofrenda:
                    RefreshMiniGame2(_model.MiniGame2);
                    break;
                case GamePhase.Phase3_Transito:
                    RefreshMiniGame3(_model.MiniGame3);
                    break;
            }
        }

        private void HandleIndicatorFocusChanged(int index)
        {
            RefreshPhaseIndicator();
            // Mover el cursor visual al nuevo ícono
            if (indicatorFocusCursor != null && phaseSymbolImages != null
                && index < phaseSymbolImages.Length)
            {
                indicatorFocusCursor.transform.position =
                    phaseSymbolImages[index].transform.position;
            }
        }

        // =====================================================================
        // GESTIÓN DE PANELES
        // =====================================================================

        private void ShowGamePanel(GamePhase phase)
        {
            miniGame1Panel?.SetActive(phase == GamePhase.Phase1_Inhumacion);
            miniGame2Panel?.SetActive(phase == GamePhase.Phase2_Ofrenda);
            miniGame3Panel?.SetActive(phase == GamePhase.Phase3_Transito);
            videoPanel?.SetActive(false);
            completionPanel?.SetActive(phase == GamePhase.Completed);
        }

        // =====================================================================
        // MINIJUEGO 1 – LOS CUATRO RUMBOS (INHUMACIÓN)
        // =====================================================================

        /// <summary>Actualiza puntos de progreso, texto de ayuda y estado de completado.</summary>
        private void RefreshMiniGame1(SequenceMemoryState state)
        {
            // Puntos de progreso
            if (sequenceProgressPips != null && state.sequence != null)
            {
                for (int i = 0; i < sequenceProgressPips.Length; i++)
                {
                    Image pip = sequenceProgressPips[i];
                    if (pip == null) continue;

                    bool inUse = i < state.sequence.Length;
                    pip.gameObject.SetActive(inUse);
                    if (inUse)
                        pip.color = (state.isComplete || i < state.inputProgress)
                            ? pipFilledColor
                            : pipEmptyColor;
                }
            }

            // Texto de ayuda
            if (sequenceHintText != null)
            {
                if (state.isComplete) sequenceHintText.text = "";
                else if (state.isPlayingBack) sequenceHintText.text = "Observa los rumbos…";
                else sequenceHintText.text = "Repite con la cruceta   A Ver de nuevo";
            }

            if (state.isComplete)
            {
                SetAllStones(stoneCorrectColor);
                if (!_mg1CompletionShown)
                {
                    _mg1CompletionShown = true;
                    StartCoroutine(PlayCompletionFlash(miniGame1Panel));
                }
            }
        }

        /// <summary>Feedback inmediato de cada pulsación del jugador.</summary>
        private void HandleSequenceInput(int symbol, SequenceInputResult result)
        {
            switch (result)
            {
                case SequenceInputResult.Correct:
                case SequenceInputResult.Completed:
                    StartCoroutine(FlashStone(symbol, stoneCorrectColor, pressFlashTime));
                    break;

                case SequenceInputResult.Wrong:
                    StartCoroutine(FlashAllStones(stoneWrongColor, wrongFlashTime));
                    break;
            }
        }

        private void HandleSequencePlaybackRequested(SequenceMemoryState state, bool afterMistake)
        {
            if (_sequencePlayback != null)
            {
                StopCoroutine(_sequencePlayback);
                ResetSequenceStones();
            }
            _sequencePlayback = StartCoroutine(PlaySequenceRoutine(state, afterMistake));
        }

        /// <summary>Ilumina las piedras una a una según la secuencia objetivo.</summary>
        private IEnumerator PlaySequenceRoutine(SequenceMemoryState state, bool afterMistake)
        {
            // Tras un error se deja ver el destello rojo antes de repetir
            yield return new WaitForSecondsRealtime(afterMistake ? mistakeReplayDelay : playbackStartDelay);

            for (int i = 0; i < state.sequence.Length; i++)
            {
                if (state.isComplete) break;   // p. ej. si se omitió la fase durante la animación

                Image stone = GetStone(state.sequence[i]);
                if (stone != null)
                {
                    stone.color = stoneLitColor;
                    stone.rectTransform.localScale = Vector3.one * litScale;
                }
                yield return new WaitForSecondsRealtime(stepLitTime);

                if (stone != null)
                {
                    stone.color = StoneBaseColor();
                    stone.rectTransform.localScale = Vector3.one;
                }
                yield return new WaitForSecondsRealtime(stepGapTime);
            }

            _sequencePlayback = null;
            _model.OnSequencePlaybackFinished();   // desbloquea la entrada del jugador
        }

        private IEnumerator FlashStone(int index, Color color, float duration)
        {
            Image stone = GetStone(index);
            if (stone == null) yield break;

            stone.color = color;
            stone.rectTransform.localScale = Vector3.one * litScale;
            yield return new WaitForSecondsRealtime(duration);
            stone.color = StoneBaseColor();
            stone.rectTransform.localScale = Vector3.one;
        }

        private IEnumerator FlashAllStones(Color color, float duration)
        {
            SetAllStones(color);
            yield return new WaitForSecondsRealtime(duration);
            SetAllStones(StoneBaseColor());
        }

        // ─── Helpers del reto 1 ───────────────────────────────────────────────

        private Image GetStone(int index) =>
            (sequenceStoneImages != null && index >= 0 && index < sequenceStoneImages.Length)
                ? sequenceStoneImages[index]
                : null;

        private Color StoneBaseColor() =>
            (_model != null && _model.MiniGame1.isComplete) ? stoneCorrectColor : stoneIdleColor;

        private void SetAllStones(Color color)
        {
            if (sequenceStoneImages == null) return;
            foreach (Image stone in sequenceStoneImages)
                if (stone != null) stone.color = color;
        }

        private void ResetSequenceStones()
        {
            if (sequenceStoneImages == null) return;
            foreach (Image stone in sequenceStoneImages)
            {
                if (stone == null) continue;
                stone.color = StoneBaseColor();
                stone.rectTransform.localScale = Vector3.one;
            }
        }

        // =====================================================================
        // MINIJUEGO 2 – SYMBOL MATCHING (OFRENDA)
        // =====================================================================

        private void RefreshMiniGame2(OfrendaMatchState state)
        {
            for (int i = 0; i < OfrendaMatchState.COUNT; i++)
            {
                rightOfrendaSlots[i].sprite = ofrendaSymbols[state.rightSymbols[i]];

                // Color de la ranura según su estado
                Color c;
                if (i == state.selectedIndex)
                    c = selectedSlotColor;
                else if (i == state.cursorIndex)
                    c = cursorSlotColor;
                else if (!state.isComplete && state.rightSymbols[i] == state.leftSymbols[i])
                    c = matchedSlotColor;  // feedback: ya está en su lugar
                else
                    c = normalSlotColor;

                rightOfrendaSlots[i].color = c;
            }

            if (state.isComplete)
                StartCoroutine(PlayCompletionFlash(miniGame2Panel));
        }

        // =====================================================================
        // MINIJUEGO 3 – DIAL ALIGNMENT (TRÁNSITO)
        // =====================================================================

        private void RefreshMiniGame3(DialAlignmentState state)
        {
            for (int i = 0; i < DialAlignmentState.DIAL_COUNT; i++)
            {
                // Actualizar sprite de posición del dial
                if (i < dialImages.Length && dialImages[i] != null)
                    dialImages[i].sprite = dialStepSprites[state.dialValues[i]];

                // Resaltar el dial activo
                if (i < dialFrames.Length && dialFrames[i] != null)
                    dialFrames[i].color = i == state.activeDial
                        ? activeDialColor
                        : inactiveDialColor;
            }

            if (state.isComplete)
                StartCoroutine(PlayCompletionFlash(miniGame3Panel));
        }

        // =====================================================================
        // INDICADOR DE FASES
        // =====================================================================

        public void RefreshPhaseIndicator()
        {
            if (_model == null) return;
            bool browsing = _model.RitualState == RitualState.BrowsingIndicator;

            for (int i = 0; i < 3; i++)
            {
                PhaseData pd = _model.phases[i];

                Color c;
                if (!pd.isUnlocked) c = lockedColor;
                else if (pd.isCompleted) c = completedColor;
                else c = unlockedColor;
                if (browsing && i == _model.IndicatorFocusIndex) c = focusedColor;

                if (i < phaseSymbolImages.Length && phaseSymbolImages[i] != null)
                    phaseSymbolImages[i].color = c;
                if (phaseSymbolFrames != null && i < phaseSymbolFrames.Length
                                              && phaseSymbolFrames[i] != null)
                    phaseSymbolFrames[i].color = c;
            }

            // Etiqueta con el nombre de la fase enfocada
            if (browsing && phaseNameLabel != null)
                phaseNameLabel.text = _model.phases[_model.IndicatorFocusIndex].name;
        }

        // =====================================================================
        // VIDEO
        // =====================================================================

        private void PlayPhaseVideo(int phaseIndex)
        {
            VideoClip clip = _model.phases[phaseIndex].videoClip;

            if (clip == null)
            {
                // Sin video asignado → avanzar de inmediato
                _model.OnVideoFinished();
                return;
            }

            // Ocultar paneles de juego, mostrar panel de video
            miniGame1Panel?.SetActive(false);
            miniGame2Panel?.SetActive(false);
            miniGame3Panel?.SetActive(false);
            videoPanel?.SetActive(true);

            videoPlayer.clip = clip;
            videoPlayer.loopPointReached += OnVideoLoopPointReached;
            videoPlayer.Play();

            if (skipHintText != null) skipHintText.text = "Mantén B para saltar";
            if (videoProgressBar != null)
            {
                videoProgressBar.value = 0f;
                StartCoroutine(UpdateVideoProgress());
            }
        }

        private void OnVideoLoopPointReached(VideoPlayer vp)
        {
            vp.loopPointReached -= OnVideoLoopPointReached;
            StopAllCoroutines(); // detener barra de progreso si sigue activa
            videoPanel?.SetActive(false);
            _model.OnVideoFinished();
        }

        /// <summary>Llamado por el Controlador cuando el jugador mantiene B.</summary>
        public void SkipVideo()
        {
            videoPlayer.loopPointReached -= OnVideoLoopPointReached;
            StopAllCoroutines();
            videoPlayer.Stop();
            videoPanel?.SetActive(false);
            _model.OnVideoFinished();
        }

        private IEnumerator UpdateVideoProgress()
        {
            while (videoPlayer != null && videoPlayer.isPlaying)
            {
                if (videoProgressBar != null && videoPlayer.frameCount > 0)
                    videoProgressBar.value = (float)(videoPlayer.frame / (double)videoPlayer.frameCount);
                yield return null;
            }
        }

        // =====================================================================
        // PANTALLA DE COMPLETADO
        // =====================================================================

        private void ShowCompletion()
        {
            miniGame1Panel?.SetActive(false);
            miniGame2Panel?.SetActive(false);
            miniGame3Panel?.SetActive(false);
            videoPanel?.SetActive(false);
            completionPanel?.SetActive(true);
        }

        // =====================================================================
        // EFECTOS VISUALES
        // =====================================================================

        /// <summary>Parpadeo rápido del panel para indicar que el minijuego se completó.</summary>
        private IEnumerator PlayCompletionFlash(GameObject panel)
        {
            if (panel == null) yield break;
            CanvasGroup cg = panel.GetComponent<CanvasGroup>();
            if (cg == null) cg = panel.AddComponent<CanvasGroup>();

            for (int i = 0; i < 3; i++)
            {
                cg.alpha = 0.25f;
                yield return new WaitForSecondsRealtime(0.08f);
                cg.alpha = 1f;
                yield return new WaitForSecondsRealtime(0.08f);
            }
        }
    }
}