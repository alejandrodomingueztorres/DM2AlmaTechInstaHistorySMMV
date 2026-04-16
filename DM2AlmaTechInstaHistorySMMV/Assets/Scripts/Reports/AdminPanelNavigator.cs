using UnityEngine;

public class AdminPanelNavigator : MonoBehaviour
{
    [Header("Paneles")]
    [SerializeField] private GameObject panelReportes;
    [SerializeField] private GameObject panelAudio;
    [SerializeField] private GameObject panelSubtitulos;
    [SerializeField] private GameObject panelModelo3D;

    private void Start()
    {
        ShowReportes();
    }

    public void ShowReportes()
    {
        HideAllPanels();
        if (panelReportes != null) panelReportes.SetActive(true);
    }

    public void ShowAudio()
    {
        HideAllPanels();
        if (panelAudio != null) panelAudio.SetActive(true);
    }

    public void ShowSubtitulos()
    {
        HideAllPanels();
        if (panelSubtitulos != null) panelSubtitulos.SetActive(true);
    }

    public void ShowModelo3D()
    {
        HideAllPanels();
        if (panelModelo3D != null) panelModelo3D.SetActive(true);
    }

    private void HideAllPanels()
    {
        if (panelReportes != null) panelReportes.SetActive(false);
        if (panelAudio != null) panelAudio.SetActive(false);
        if (panelSubtitulos != null) panelSubtitulos.SetActive(false);
        if (panelModelo3D != null) panelModelo3D.SetActive(false);
    }
}