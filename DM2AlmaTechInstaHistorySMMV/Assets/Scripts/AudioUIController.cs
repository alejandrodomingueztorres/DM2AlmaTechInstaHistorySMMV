using UnityEngine;
using UnityEngine.UI;

public class AudioUIController : MonoBehaviour
{
    [Header("Puzzle Controller")]
    // Referencia al PuzzleController para controlar las narraciones y usar su cursor virtual
    public PuzzleController puzzleController;

    [Header("Buttons")]
    public GameObject btnPlay;
    public GameObject btnPause;
    public GameObject btnSkip;   // Botón para omitir la narración activa (puede ser null)

    private bool isPaused = false;

    void OnEnable()
    {
        if (puzzleController != null)
            puzzleController.OnCursorClick += HandleCursorClick;
    }

    void OnDisable()
    {
        if (puzzleController != null)
            puzzleController.OnCursorClick -= HandleCursorClick;
    }

    // ================================
    // DETECCIÓN DE CLIC CON EL CURSOR
    // ================================

    // Se llama desde el evento OnCursorClick del PuzzleController cada vez que
    // el jugador presiona el botón de agarrar. Comprueba si el cursor está
    // encima de alguno de los botones de audio y ejecuta la acción correspondiente.
    void HandleCursorClick()
    {
        if (btnPause != null && btnPause.activeSelf && IsCursorOver(btnPause))
        {
            Pause();
            return;
        }

        if (btnPlay != null && btnPlay.activeSelf && IsCursorOver(btnPlay))
        {
            Play();
            return;
        }

        if (btnSkip != null && btnSkip.activeSelf && IsCursorOver(btnSkip))
        {
            SkipNarration();
        }
    }

    // Devuelve true si la posición actual del cursor cae dentro del RectTransform del botón
    bool IsCursorOver(GameObject btn)
    {
        RectTransform rt = btn.GetComponent<RectTransform>();
        if (rt == null) return false;

        // Canvas en Screen Space - Overlay: null como cámara es suficiente.
        // Si el canvas usa Screen Space - Camera, pasa su cámara en lugar de null.
        return RectTransformUtility.RectangleContainsScreenPoint(rt, puzzleController.CursorPosition, null);
    }

    // ================================
    // CONTROLES DE AUDIO
    // ================================

    public void Play()
    {
        if (puzzleController == null) return;

        if (isPaused)
            puzzleController.ReanudarNarracion();
        // Si no estaba pausado no hay narración activa que reanudar;
        // las narraciones se disparan automáticamente al colocar piezas.

        isPaused = false;

        btnPlay.SetActive(false);
        btnPause.SetActive(true);
    }

    public void Pause()
    {
        if (puzzleController == null) return;

        puzzleController.PausarNarracion();

        isPaused = true;

        btnPlay.SetActive(true);
        btnPause.SetActive(false);
    }

    // Omite (detiene) la narración activa; el PuzzleController limpia su estado solo
    public void SkipNarration()
    {
        if (puzzleController == null) return;

        puzzleController.OmitirNarracion();

        isPaused = false;

        btnPlay.SetActive(true);
        btnPause.SetActive(false);
    }

    // Restablece el estado visual de los botones cuando empieza una nueva narración.
    // Llama a este método desde PuzzleController si quieres que el botón Pause
    // aparezca automáticamente al iniciar cada narración.
    public void OnNarracionIniciada()
    {
        isPaused = false;
        btnPlay.SetActive(false);
        btnPause.SetActive(true);
    }
}