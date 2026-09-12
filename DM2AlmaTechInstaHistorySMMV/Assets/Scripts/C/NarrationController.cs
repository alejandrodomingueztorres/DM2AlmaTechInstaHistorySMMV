using UnityEngine;

public class NarrationController : MonoBehaviour
{
    public AudioSource currentAudio;

    public void SetCurrentAudio(AudioSource audio)
    {
        currentAudio = audio;
    }

    public void PauseNarration()
    {
        if (currentAudio != null && currentAudio.isPlaying)
        {
            currentAudio.Pause();
        }
    }

    public void ResumeNarration()
    {
        if (currentAudio != null)
        {
            currentAudio.UnPause();
        }
    }

    public void SkipNarration()
    {
        if (currentAudio != null)
        {
            currentAudio.Stop();
        }
    }
}