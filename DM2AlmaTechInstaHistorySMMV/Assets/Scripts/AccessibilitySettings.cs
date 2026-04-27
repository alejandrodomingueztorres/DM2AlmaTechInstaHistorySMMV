using UnityEngine;
using UnityEngine.UI;

public class AccessibilitySettings : MonoBehaviour
{

    public Toggle toggleSubtitulos;
    public Toggle toggleAudiodesc;

    public static bool subtitulosActivos = true;
    public static bool audiodescActiva = false;

    void Start()
    {
        toggleSubtitulos.isOn = subtitulosActivos;
        toggleAudiodesc.isOn = audiodescActiva;

        toggleSubtitulos.onValueChanged.AddListener(SetSubtitulos);
        toggleAudiodesc.onValueChanged.AddListener(SetAudiodesc);
    }

    void SetSubtitulos(bool value)
    {
        subtitulosActivos = value;
    }

    void SetAudiodesc(bool value)
    {
        audiodescActiva = value;
    }
}