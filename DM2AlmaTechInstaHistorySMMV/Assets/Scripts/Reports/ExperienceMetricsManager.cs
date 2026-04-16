using UnityEngine;

public class ExperienceMetricsManager : MonoBehaviour
{
    public static ExperienceMetricsManager Instance;

    [Header("Métricas acumuladas")]
    [SerializeField] private int totalVisitors = 0;
    [SerializeField] private float totalSessionDuration = 0f;
    [SerializeField] private int totalInteractions = 0;
    [SerializeField] private int completedStages = 0;

    private float currentSessionStartTime;
    private bool sessionRunning = false;

    public int TotalVisitors => totalVisitors;
    public int TotalInteractions => totalInteractions;
    public int CompletedStages => completedStages;

    public float AverageSessionDuration
    {
        get
        {
            if (totalVisitors == 0) return 0f;
            return totalSessionDuration / totalVisitors;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void RegisterVisitor()
    {
        totalVisitors++;
    }

    public void StartSession()
    {
        currentSessionStartTime = Time.time;
        sessionRunning = true;
    }

    public void EndSession()
    {
        if (!sessionRunning) return;

        float sessionDuration = Time.time - currentSessionStartTime;
        totalSessionDuration += sessionDuration;
        sessionRunning = false;
    }

    public void RegisterInteraction()
    {
        totalInteractions++;
    }

    public void RegisterCompletedStage()
    {
        completedStages++;
    }

    public void ResetMetrics()
    {
        totalVisitors = 0;
        totalSessionDuration = 0f;
        totalInteractions = 0;
        completedStages = 0;
        currentSessionStartTime = 0f;
        sessionRunning = false;
    }
}