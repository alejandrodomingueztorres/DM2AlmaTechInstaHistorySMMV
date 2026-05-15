using UnityEngine;
using UnityEngine.EventSystems;

public class MenuManager : MonoBehaviour
{
    public GameObject panelMenu;
    public GameObject panelAjustes;
    public GameObject menubtn;
    public GameObject ajustesbtn;

    public void AbrirAjustes()
    {
        panelMenu.SetActive(false);
        panelAjustes.SetActive(true);
        EventSystem.current.SetSelectedGameObject(ajustesbtn);
    }

    public void CerrarAjustes()
    {
        panelMenu.SetActive(true);
        panelAjustes.SetActive(false);
        EventSystem.current.SetSelectedGameObject(menubtn);
    }
}
