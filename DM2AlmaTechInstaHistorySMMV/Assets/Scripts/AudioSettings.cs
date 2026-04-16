using UnityEngine;
using UnityEngine.UI;
public class AudioSettings : MonoBehaviour
{
    public Toggle toggleSubtitulos;
    public Toggle toggleAudio;

    public bool subtitulosActivos;
    public bool audioDescripcionActiva;

    void Start()
    {
        toggleSubtitulos.onValueChanged.AddListener(SetSubtitulos);
        toggleAudio.onValueChanged.AddListener(SetAudio);
    }

    void SetSubtitulos(bool estado)
    {
        subtitulosActivos = estado;
    }

    void SetAudio(bool estado)
    {
        audioDescripcionActiva = estado;
    }
}
