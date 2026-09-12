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
    public static SubtitleManager Instance;

    [Header("Narraciones")]
    public List<NarracionEntry> narraciones;

    [Header("UI")]
    public TextMeshProUGUI subtitleText;
    public float fadeSpeed = 3f;

    private List<SubtitleEntry> subtitulosActivos;
    private NarracionEntry narracionActual;
    private CanvasGroup canvasGroup;

    private Dictionary<string, List<SubtitleEntry>> cache =
        new Dictionary<string, List<SubtitleEntry>>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        canvasGroup = subtitleText.GetComponentInParent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = subtitleText.gameObject.AddComponent<CanvasGroup>();

        subtitleText.text = "";
        canvasGroup.alpha = 0f;

        // Precargar todos los SRT
        foreach (var n in narraciones)
        {
            if (!string.IsNullOrEmpty(n.srtFileName) &&
                !cache.ContainsKey(n.srtFileName))
            {
                var srt = CargarSRT(n.srtFileName);

                if (srt != null)
                    cache[n.srtFileName] = srt;
            }
        }
    }

    void Update()
    {
        // DEBUG TEMPORAL
        foreach (var n in narraciones)
        {
            if (n.audioSource != null &&
                n.audioSource.isPlaying &&
                n.audioSource.time > 0f)
            {
                Debug.Log(
                    n.srtFileName +
                    " | time: " +
                    n.audioSource.time.ToString("F1")
                );
            }
        }

        // Accesibilidad
        if (!AccessibilitySettings.subtitulosActivos)
        {
            subtitleText.text = "";
            canvasGroup.alpha = 0f;
            return;
        }

        NarracionEntry activa = GetNarracionSonando();

        // Cambio de narración
        if (activa != narracionActual)
        {
            narracionActual = activa;

            subtitulosActivos =
                activa != null &&
                cache.ContainsKey(activa.srtFileName)
                ? cache[activa.srtFileName]
                : null;
        }

        // Mostrar subtítulo
        if (subtitulosActivos != null &&
            narracionActual != null)
        {
            float tiempo = narracionActual.audioSource.time;

            SubtitleEntry entrada =
                GetSubtituloActivo(tiempo);

            if (entrada != null)
            {
                subtitleText.text = entrada.text;

                canvasGroup.alpha =
                    Mathf.MoveTowards(
                        canvasGroup.alpha,
                        1f,
                        fadeSpeed * Time.deltaTime
                    );
            }
            else
            {
                subtitleText.text = "";

                canvasGroup.alpha =
                    Mathf.MoveTowards(
                        canvasGroup.alpha,
                        0f,
                        fadeSpeed * Time.deltaTime
                    );
            }
        }
        else
        {
            subtitleText.text = "";

            canvasGroup.alpha =
                Mathf.MoveTowards(
                    canvasGroup.alpha,
                    0f,
                    fadeSpeed * Time.deltaTime
                );
        }
    }

    public void SetActive(bool value)
    {
        if (!value)
        {
            subtitleText.text = "";
            canvasGroup.alpha = 0f;
        }
    }

    private NarracionEntry GetNarracionSonando()
    {
        foreach (var n in narraciones)
        {
            if (n.audioSource != null &&
                n.audioSource.isPlaying &&
                n.audioSource.time > 0f)
            {
                if (cache.ContainsKey(n.srtFileName))
                {
                    var entrada =
                        GetSubtituloActivoEnLista(
                            cache[n.srtFileName],
                            n.audioSource.time
                        );

                    if (entrada != null)
                        return n;
                }
            }
        }

        return null;
    }

    private List<SubtitleEntry> CargarSRT(string nombreArchivo)
    {
        string path =
            Path.Combine(
                Application.streamingAssetsPath,
                nombreArchivo
            );

        Debug.Log("[Subtitles] Buscando archivo: " + path);

        if (File.Exists(path))
        {
            var lista = SRTParser.Parse(path);

            if (lista != null)
            {
                Debug.Log(
                    "[Subtitles] Cargado correctamente: " +
                    nombreArchivo
                );

                Debug.Log(
                    "[Subtitles] Cantidad de entradas: " +
                    lista.Count
                );
            }
            else
            {
                Debug.LogError(
                    "[Subtitles] Error parseando: " +
                    nombreArchivo
                );
            }

            return lista;
        }

        Debug.LogWarning(
            "[Subtitles] Archivo no encontrado: " +
            path
        );

        return null;
    }

    private SubtitleEntry GetSubtituloActivo(float tiempo)
    {
        return GetSubtituloActivoEnLista(
            subtitulosActivos,
            tiempo
        );
    }

    private SubtitleEntry GetSubtituloActivoEnLista(
        List<SubtitleEntry> lista,
        float tiempo
    )
    {
        foreach (var entry in lista)
        {
            Debug.Log(
                "Tiempo audio: " +
                tiempo.ToString("F2") +
                " | Subtitle: " +
                entry.startTime.ToString("F2") +
                " -> " +
                entry.endTime.ToString("F2")
            );

            if (tiempo >= entry.startTime &&
                tiempo <= entry.endTime)
            {
                Debug.Log(
                    "SUBTITULO ACTIVO: " +
                    entry.text
                );

                return entry;
            }
        }

        return null;
    }
}