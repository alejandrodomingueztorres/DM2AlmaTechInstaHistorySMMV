using UnityEngine;
using UnityEngine.InputSystem;

public class TestClosingScreen : MonoBehaviour
{
    private ClosingScreen closingScreen;

    void Start()
    {
        closingScreen = GetComponent<ClosingScreen>();
    }

    void Update()
    {
        // Click del mouse (para probar en PC)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            closingScreen.ShowClosingScreen();
        }

        // Toque en pantalla táctil (tablet)
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            closingScreen.ShowClosingScreen();
        }
    }
}