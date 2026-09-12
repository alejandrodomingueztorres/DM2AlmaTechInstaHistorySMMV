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


    private Coroutine flashCoroutine;

    private Renderer[] renderers;
    private Color[] originalColors;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();

        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material.HasProperty("_Color"))
                originalColors[i] = renderers[i].material.color;
        }
    }

    public void PlayCorrectFeedback()
    {
        if (correctParticles) correctParticles.Play();

        if (audioSource && correctSound)
            audioSource.PlayOneShot(correctSound);

        if (feedbackLight)
        {
            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);

            flashCoroutine = StartCoroutine(FlashLight(Color.green));
        }
    }

    public void PlayWrongFeedback()
    {
        if (audioSource && wrongSound)
            audioSource.PlayOneShot(wrongSound);

        if (feedbackLight)
        {
            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);

            flashCoroutine = StartCoroutine(FlashLight(Color.red));
        }
    }

    public void SetSectionHighlight(bool active)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (!renderers[i].material.HasProperty("_Color")) continue;

            if (active)
                renderers[i].material.color = new Color(0.85f, 0.75f, 0.25f, 1f);
            else
                renderers[i].material.color = originalColors[i];
        }
    }

    private System.Collections.IEnumerator FlashLight(Color color)
    {
        feedbackLight.color = color;
        feedbackLight.enabled = true;

        yield return new WaitForSeconds(0.3f);

        feedbackLight.enabled = false;
        flashCoroutine = null;
    }
      
}