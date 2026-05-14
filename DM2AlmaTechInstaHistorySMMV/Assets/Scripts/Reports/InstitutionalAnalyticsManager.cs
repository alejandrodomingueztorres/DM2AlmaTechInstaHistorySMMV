using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InstitutionalAnalyticsManager : MonoBehaviour
{
    [Header("Texto resumen")]
    [SerializeField] private TMP_Text textoUsuariosSemana;
    [SerializeField] private TMP_Text textoSeccionMasVista;

    [Header("Barras")]
    [SerializeField] private RectTransform barraPuzzle;
    [SerializeField] private RectTransform barra3DView;
    [SerializeField] private RectTransform barraInstitucional;

    [Header("Labels barras")]
    [SerializeField] private TMP_Text txtPuzzleValue;
    [SerializeField] private TMP_Text txt3DViewValue;
    [SerializeField] private TMP_Text txtInstitucionalValue;

    [Header("Filtros")]
    [SerializeField] private TMP_Dropdown filtroTiempoDropdown;
    [SerializeField] private TMP_Dropdown filtroUsuarioDropdown;

    [Header("Escala visual")]
    [SerializeField] private float maxBarWidth = 300f;
    [SerializeField] private float minBarWidth = 12f;

    private int puzzleInteractions;
    private int view3DInteractions;
    private int institutionalVisits;

    private void Start()
    {
        CargarMetricas();
        ConfigurarDropdowns();
        ActualizarGraficas();
    }

    private void CargarMetricas()
    {
        puzzleInteractions = PlayerPrefs.GetInt("Metrics_TotalInteractions", 0);
        view3DInteractions = PlayerPrefs.GetInt("Metrics_3DViewInteractions", 0);
        institutionalVisits = PlayerPrefs.GetInt("Metrics_InstitutionalVisits", 0);

        institutionalVisits++;
        PlayerPrefs.SetInt("Metrics_InstitutionalVisits", institutionalVisits);
        PlayerPrefs.Save();

        Debug.Log("[InstitutionalAnalytics] Métricas institucionales cargadas.");
    }

    private void ConfigurarDropdowns()
    {
        if (filtroTiempoDropdown != null)
        {
            filtroTiempoDropdown.ClearOptions();
            filtroTiempoDropdown.AddOptions(new List<string> { "Semana", "Mes", "Total" });
            filtroTiempoDropdown.onValueChanged.AddListener(_ => AplicarFiltros());
        }

        if (filtroUsuarioDropdown != null)
        {
            filtroUsuarioDropdown.ClearOptions();
            filtroUsuarioDropdown.AddOptions(new List<string> { "Todos", "Institucional", "Administrativo" });
            filtroUsuarioDropdown.onValueChanged.AddListener(_ => AplicarFiltros());
        }
    }

    private void AplicarFiltros()
    {
        Debug.Log("[InstitutionalAnalytics] Filtros aplicados.");
        ActualizarGraficas();
    }

    private void ActualizarGraficas()
    {
        int totalVisitors = PlayerPrefs.GetInt("Metrics_TotalVisitors", 0);

        int maxValue = Mathf.Max(puzzleInteractions, view3DInteractions, institutionalVisits, 1);

        SetBar(barraPuzzle, puzzleInteractions, maxValue);
        SetBar(barra3DView, view3DInteractions, maxValue);
        SetBar(barraInstitucional, institutionalVisits, maxValue);

        if (txtPuzzleValue != null)
            txtPuzzleValue.text = "Puzzle interactivo: " + puzzleInteractions;

        if (txt3DViewValue != null)
            txt3DViewValue.text = "Exploración 3D: " + view3DInteractions;

        if (txtInstitucionalValue != null)
            txtInstitucionalValue.text = "Panel institucional: " + institutionalVisits;

        if (textoUsuariosSemana != null)
            textoUsuariosSemana.text = "Usuarios/Sesiones registradas: " + totalVisitors;

        if (textoSeccionMasVista != null)
            textoSeccionMasVista.text = "Sección más visitada: " + ObtenerSeccionMasVista();

        Debug.Log("[InstitutionalAnalytics] Gráficas actualizadas.");
    }

    private void SetBar(RectTransform bar, int value, int maxValue)
    {
        if (bar == null) return;

        float normalized = value / (float)maxValue;
        float width = value <= 0 ? minBarWidth : Mathf.Max(minBarWidth, normalized * maxBarWidth);

        bar.sizeDelta = new Vector2(width, bar.sizeDelta.y);
    }

    private string ObtenerSeccionMasVista()
    {
        if (puzzleInteractions >= view3DInteractions && puzzleInteractions >= institutionalVisits)
            return "Puzzle interactivo";

        if (view3DInteractions >= institutionalVisits)
            return "Exploración 3D";

        return "Panel institucional";
    }
}