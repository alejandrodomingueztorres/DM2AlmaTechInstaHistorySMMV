using UnityEngine;

public class AudioAccessibilityManager : MonoBehaviour
{
    public static AudioAccessibilityManager instance;

    public bool audiodescripcionActiva = true;
    public bool subtitulosActivos = true;

    void Awake()
    {
        instance = this;
    }

    public void ToggleAudiodescripcion(bool value)
    {
        audiodescripcionActiva = value;
    }

    public void ToggleSubtitulos(bool value)
    {
        subtitulosActivos = value;
    }
}
