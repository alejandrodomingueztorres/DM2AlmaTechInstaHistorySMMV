using UnityEngine;

public class MetricsManager : MonoBehaviour
{
    public static MetricsManager Instance;

    private const string TotalVisitorsKey = "Metrics_TotalVisitors";
    private const string TotalSessionDurationKey = "Metrics_TotalSessionDuration";
    private const string TotalInteractionsKey = "Metrics_TotalInteractions";
    private const string CompletedStagesKey = "Metrics_CompletedStages";

    private float sessionStartTime;
    private bool sessionActive;

    public int TotalVisitors => PlayerPrefs.GetInt(TotalVisitorsKey, 0);
    public float TotalSessionDuration => PlayerPrefs.GetFloat(TotalSessionDurationKey, 0f);
    public int TotalInteractions => PlayerPrefs.GetInt(TotalInteractionsKey, 0);
    public int CompletedStages => PlayerPrefs.GetInt(CompletedStagesKey, 0);

    public float CurrentSessionDuration => sessionActive ? Time.time - sessionStartTime : 0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[Metrics] MetricsManager inicializado correctamente.");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        StartSession();
    }

    public void StartSession()
    {
        sessionStartTime = Time.time;
        sessionActive = true;

        int visitors = PlayerPrefs.GetInt(TotalVisitorsKey, 0);
        visitors++;
        PlayerPrefs.SetInt(TotalVisitorsKey, visitors);
        PlayerPrefs.Save();

        Debug.Log("[Metrics] Sesión iniciada. Visitantes acumulados: " + visitors);
    }

    public void RegisterInteraction(string interactionName = "Interacción")
    {
        int interactions = PlayerPrefs.GetInt(TotalInteractionsKey, 0);
        interactions++;
        PlayerPrefs.SetInt(TotalInteractionsKey, interactions);
        PlayerPrefs.Save();

        Debug.Log("[Metrics] Interacción registrada: " + interactionName + " | Total: " + interactions);
    }

    public void CompleteStage(string stageName)
    {
        int stages = PlayerPrefs.GetInt(CompletedStagesKey, 0);
        stages++;
        PlayerPrefs.SetInt(CompletedStagesKey, stages);
        PlayerPrefs.Save();

        Debug.Log("[Metrics] Etapa completada: " + stageName + " | Total etapas: " + stages);
    }

    public void EndSession()
    {
        if (!sessionActive) return;

        float duration = Time.time - sessionStartTime;
        float totalDuration = PlayerPrefs.GetFloat(TotalSessionDurationKey, 0f);
        totalDuration += duration;

        PlayerPrefs.SetFloat(TotalSessionDurationKey, totalDuration);
        PlayerPrefs.Save();

        sessionActive = false;

        Debug.Log("[Metrics] Sesión finalizada. Duración añadida: " + duration.ToString("F2") + " segundos.");
        Debug.Log("[Metrics] Duración acumulada: " + totalDuration.ToString("F2") + " segundos.");
    }

    public void ResetMetrics()
    {
        PlayerPrefs.DeleteKey(TotalVisitorsKey);
        PlayerPrefs.DeleteKey(TotalSessionDurationKey);
        PlayerPrefs.DeleteKey(TotalInteractionsKey);
        PlayerPrefs.DeleteKey(CompletedStagesKey);
        PlayerPrefs.Save();

        Debug.Log("[Metrics] Métricas reiniciadas.");
    }
}