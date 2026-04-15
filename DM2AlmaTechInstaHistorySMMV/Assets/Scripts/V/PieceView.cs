using UnityEngine;

public class PieceView : MonoBehaviour
{
    public ParticleSystem correctParticles;
    public AudioSource audioSource;
    public AudioClip correctSound;
    public AudioClip wrongSound;

    // CORREGIDO: feedbackLight puede no estar asignado; todas las
    // referencias se validan antes de usarse.
    public Light feedbackLight;

    public void PlayCorrectFeedback()
    {
        if (correctParticles) correctParticles.Play();

        if (audioSource && correctSound)
            audioSource.PlayOneShot(correctSound);

        if (feedbackLight)
            StartCoroutine(FlashLight(Color.green));
    }

    public void PlayWrongFeedback()
    {
        if (audioSource && wrongSound)
            audioSource.PlayOneShot(wrongSound);

        if (feedbackLight)
            StartCoroutine(FlashLight(Color.red));
    }

    private System.Collections.IEnumerator FlashLight(Color color)
    {
        feedbackLight.color = color;
        feedbackLight.enabled = true;

        yield return new WaitForSeconds(0.3f);

        feedbackLight.enabled = false;
    }
}