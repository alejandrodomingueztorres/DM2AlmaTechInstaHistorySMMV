using UnityEngine;

public class AccessibilityManager : MonoBehaviour
{
    public static AccessibilityManager Instance;

    [Header("Estados")]
    public bool subtitlesEnabled = true;
    public bool audioDescriptionEnabled = true;

    private void Awake()
    {
        Instance = this;
    }

    // SUBTITULOS
    public void SetSubtitles(bool value)
    {
        subtitlesEnabled = value;

        SubtitleManager.Instance.SetActive(value);
    }

    // AUDIO DESCRIPCION
    public void SetAudioDescription(bool value)
    {
        audioDescriptionEnabled = value;
    }
}
