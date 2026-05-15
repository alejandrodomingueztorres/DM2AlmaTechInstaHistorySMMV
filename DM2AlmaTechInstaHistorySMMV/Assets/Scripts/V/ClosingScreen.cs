using UnityEngine;
using TMPro;

public class ClosingScreen : MonoBehaviour
{
    [Header("UI Referencias")]
    public GameObject closingPanel;
    public TextMeshProUGUI messageText;

    [Header("Configuración")]
    [TextArea]
    public string finalMessage = "La vida de los muertos perdura en la memoria de los vivos.";

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip ritualTone;

    void Start()
    {
        ShowClosingScreen();
    }

    private void ShowClosingScreen()
    {
        // Activa panel inmediatamente
        closingPanel.SetActive(true);

        // Asigna texto
        messageText.text = finalMessage;

        // Reproduce audio
        if (ritualTone != null && audioSource != null)
        {
            audioSource.PlayOneShot(ritualTone);
        }
    }
}
