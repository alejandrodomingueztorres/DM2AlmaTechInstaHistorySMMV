using UnityEngine;
using UnityEngine.InputSystem;

public class MenuToggleController : MonoBehaviour
{
    public GameObject menuRoot;
    public GameObject panelPrincipal;
    public GameObject panelAjustes;

    bool menuActivo = false;

    void Update()
    {
        if (Gamepad.current == null) return;

        if (Gamepad.current.startButton.wasPressedThisFrame)
        {
            ToggleMenu();
        }
    }

    void ToggleMenu()
    {
        menuActivo = !menuActivo;
        menuRoot.SetActive(menuActivo);

        if (menuActivo)
        {
            panelPrincipal.SetActive(true);
            panelAjustes.SetActive(false);
        }
    }

    public void Volver()
    {
        if (panelAjustes.activeSelf)
        {
            panelAjustes.SetActive(false);
            panelPrincipal.SetActive(true);
        }
        else
        {
            menuRoot.SetActive(false);
            menuActivo = false;
        }
    }

    public void AbrirAjustes()
    {
        panelPrincipal.SetActive(false);
        panelAjustes.SetActive(true);
    }
}
