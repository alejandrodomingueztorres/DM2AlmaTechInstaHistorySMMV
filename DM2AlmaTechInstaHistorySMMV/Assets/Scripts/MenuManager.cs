using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject panelMenu;
    public GameObject panelAjustes;

    public void AbrirAjustes()
    {
        panelMenu.SetActive(false);
        panelAjustes.SetActive(true);
    }

    public void CerrarAjustes()
    {
        panelMenu.SetActive(true);
        panelAjustes.SetActive(false);
    }
}
