using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RecognitionEffect : MonoBehaviour
{
    [Header("Flash Visual")]
    public Image flashOverlay;
    public float flashDuration = 0.3f;
    public AnimationCurve flashCurve;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip ritualTone;

    [Header("Sincronización")]
    public float delayBetweenFlashes = 0.1f;
    public int numberOfFlashes = 2;

    public void TriggerEffect()
    {
        StartCoroutine(PlayFlashSequence());

        if (ritualTone != null)
            audioSource.PlayOneShot(ritualTone);
    }

    private IEnumerator PlayFlashSequence()
    {
        for (int i = 0; i < numberOfFlashes; i++)
        {
            yield return StartCoroutine(SingleFlash());
            yield return new WaitForSeconds(delayBetweenFlashes);
        }
    }

    private IEnumerator SingleFlash()
    {
        float elapsed = 0f;
        Color flashColor = flashOverlay.color;

        while (elapsed < flashDuration)
        {
            float t = elapsed / flashDuration;
            float alpha = flashCurve.Evaluate(t);
            flashOverlay.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        flashOverlay.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);
    }
}
