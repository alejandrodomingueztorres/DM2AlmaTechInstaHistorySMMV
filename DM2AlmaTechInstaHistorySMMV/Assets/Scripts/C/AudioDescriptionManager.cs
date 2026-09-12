using UnityEngine;
using System.Collections;

public class AudioDescriptionManager : MonoBehaviour
{
    public AudioSource audioDescription;
    public float delay = 1f;

    private Coroutine routine;
    private bool hasPlayed = false;

    void Start()
    {
        if (AccessibilitySettings.audiodescActiva)
        {
            StartAudioSequence();
        }
    }

    public void StartAudioSequence()
    {
        if (routine != null)
            StopCoroutine(routine);

        hasPlayed = false;
        routine = StartCoroutine(PlayAfterDelay());
    }

    IEnumerator PlayAfterDelay()
    {
        yield return new WaitForSeconds(delay);

        if (AccessibilitySettings.audiodescActiva && !hasPlayed)
        {
            Debug.Log("Reproduciendo audiodescripción");
            audioDescription.Play();
            hasPlayed = true;
        }
    }

    public void StopAudio()
    {
        if (routine != null)
            StopCoroutine(routine);

        if (audioDescription.isPlaying)
        {
            Debug.Log("Audio detenido");
            audioDescription.Stop();
        }

        hasPlayed = false;
    }
}
