using UnityEngine;

public class AudioDescriptionPlayer : MonoBehaviour
{
    public AudioSource source;

    public void Play()
    {
        if (!AccessibilitySettings.audiodescActiva)
            return;

        source.Play();
    }

    public void Stop()
    {
        source.Stop();
    }
}
