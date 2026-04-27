using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuToggleController : MonoBehaviour
{
    public GameObject menuRoot;
    public GameObject panelPrincipal;
    public GameObject panelAjustes;

    [Header("First Selected")]
    public Button firstMenuButton;
    public Button firstAjustesButton;


    bool menuActivo = false;

    void Update()
    {
        if (Gamepad.current != null &&
            Gamepad.current.startButton.wasPressedThisFrame)
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

            EventSystem.current.SetSelectedGameObject(firstMenuButton.gameObject);
        }
    }

    public void AbrirAjustes()
    {
        panelPrincipal.SetActive(false);
        panelAjustes.SetActive(true);

        EventSystem.current.SetSelectedGameObject(firstAjustesButton.gameObject);
    }

    public void Volver()
    {
        if (panelAjustes.activeSelf)
        {
            panelAjustes.SetActive(false);
            panelPrincipal.SetActive(true);

            EventSystem.current.SetSelectedGameObject(firstMenuButton.gameObject);
        }
        else
        {
            menuRoot.SetActive(false);
            menuActivo = false;
        }
    }
}