using UnityEngine;

public class AudioSettingsManager : MonoBehaviour
{
    public static bool subtitlesEnabled = true;
    public static bool audioDescEnabled = true;

    public void SetSubtitles(bool value)
    {
        subtitlesEnabled = value;
        Debug.Log("Subtitulos: " + value);
    }

    public void SetAudioDescriptions(bool value)
    {
        audioDescEnabled = value;
        Debug.Log("AudioDesc: " + value);
    }
}
