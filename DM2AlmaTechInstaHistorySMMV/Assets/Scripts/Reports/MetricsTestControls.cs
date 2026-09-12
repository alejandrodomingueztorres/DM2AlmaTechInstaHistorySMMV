using UnityEngine;

public class MetricsTestControls : MonoBehaviour
{
    [ContextMenu("Test/Registrar visitante e iniciar sesión")]
    public void TestStartSession()
    {
        if (ExperienceMetricsManager.Instance == null)
        {
            Debug.LogError("No existe ExperienceMetricsManager.");
            return;
        }

        ExperienceMetricsManager.Instance.RegisterVisitor();
        ExperienceMetricsManager.Instance.StartSession();
        Debug.Log("Visitante registrado y sesión iniciada.");
    }

    [ContextMenu("Test/Registrar interacción")]
    public void TestInteraction()
    {
        if (ExperienceMetricsManager.Instance == null)
        {
            Debug.LogError("No existe ExperienceMetricsManager.");
            return;
        }

        ExperienceMetricsManager.Instance.RegisterInteraction();
        Debug.Log("Interacción registrada.");
    }

    [ContextMenu("Test/Registrar etapa completada")]
    public void TestCompletedStage()
    {
        if (ExperienceMetricsManager.Instance == null)
        {
            Debug.LogError("No existe ExperienceMetricsManager.");
            return;
        }

        ExperienceMetricsManager.Instance.RegisterCompletedStage();
        Debug.Log("Etapa completada registrada.");
    }

    [ContextMenu("Test/Finalizar sesión")]
    public void TestEndSession()
    {
        if (ExperienceMetricsManager.Instance == null)
        {
            Debug.LogError("No existe ExperienceMetricsManager.");
            return;
        }

        ExperienceMetricsManager.Instance.EndSession();
        Debug.Log("Sesión finalizada.");
    }

    [ContextMenu("Test/Resetear métricas")]
    public void TestResetMetrics()
    {
        if (ExperienceMetricsManager.Instance == null)
        {
            Debug.LogError("No existe ExperienceMetricsManager.");
            return;
        }

        ExperienceMetricsManager.Instance.ResetMetrics();
        Debug.Log("Métricas reseteadas.");
    }
}