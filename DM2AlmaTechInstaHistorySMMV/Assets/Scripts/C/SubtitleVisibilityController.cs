using UnityEngine;

public class SubtitleVisibilityController : MonoBehaviour
{
    public GameObject subtitlePanel;

    void Update()
    {
        subtitlePanel.SetActive(
            AccessibilitySettings.subtitulosActivos
        );
    }
}
