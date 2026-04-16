using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;

[System.Serializable]
public class NarracionEntry
{
    public AudioSource audioSource;
    public string srtFileName;
}

public class SubtitleManager : MonoBehaviour
{
    [Header("Narraciones")]
    public List<NarracionEntry> narraciones;

    [Header("UI")]
    public TextMeshProUGUI subtitleText;
    public float fadeSpeed = 3f;

    // Datos internos
    private List<SubtitleEntry> subtitulosActivos;
    private NarracionEntry narracionActual;
    private CanvasGroup canvasGroup;

    void Start()
    {
        canvasGroup = subtitleText.GetComponentInParent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = subtitleText.gameObject.AddComponent<CanvasGroup>();

        subtitleText.text = "";
        canvasGroup.alpha = 0f;
    }

    void Update()
    {
        // Línea temporal de debug
        Debug.Log("Narracion sonando: " + GetNarracionSonando()?.srtFileName ?? "ninguna");
        // Detectar cuál narración está sonando ahora
        NarracionEntry activa = GetNarracionSonando();

        // Si cambió la narración, cargar su SRT
        if (activa != narracionActual)
        {
            narracionActual = activa;
            subtitulosActivos = activa != null ? CargarSRT(activa.srtFileName) : null;
        }

        // Mostrar subtítulo correspondiente
        if (subtitulosActivos != null && narracionActual != null)
        {
            float tiempo = narracionActual.audioSource.time;
            SubtitleEntry entrada = GetSubtituloActivo(tiempo);

            if (entrada != null)
            {
                subtitleText.text = entrada.text;
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 1f, fadeSpeed * Time.deltaTime);
            }
            else
            {
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 0f, fadeSpeed * Time.deltaTime);
            }
        }
        else
        {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 0f, fadeSpeed * Time.deltaTime);
        }
    }

    // Devuelve la narración que está sonando en este momento
    private NarracionEntry GetNarracionSonando()
    {
        foreach (var n in narraciones)
        {
            if (n.audioSource != null && n.audioSource.isPlaying)
                return n;
        }
        return null;
    }

    // Carga el SRT de StreamingAssets
    private List<SubtitleEntry> CargarSRT(string nombreArchivo)
    {
        string path = Path.Combine(Application.streamingAssetsPath, nombreArchivo);
        if (File.Exists(path))
        {
            Debug.Log($"[Subtitles] Cargando: {nombreArchivo}");
            return SRTParser.Parse(path);
        }
        Debug.LogWarning($"[Subtitles] Archivo no encontrado: {path}");
        return null;
    }

    // Busca el subtítulo activo según el tiempo del audio
    private SubtitleEntry GetSubtituloActivo(float tiempo)
    {
        foreach (var entry in subtitulosActivos)
        {
            if (tiempo >= entry.startTime && tiempo <= entry.endTime)
                return entry;
        }
        return null;
    }
}