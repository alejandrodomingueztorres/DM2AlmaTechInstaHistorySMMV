using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.UI;
using System.Collections;
using static ObjectModel;

public class ObjectView : MonoBehaviour
{
    public Transform horizontalPivot; // Y world
    public Transform verticalPivot;   // X local
    public Transform targetCamera;
    public Light Spotlight;
    public Volume volume;
    public ColorAdjustments colorAdjustments;
    public float minLight = 0f;
    public float maxLight = 370f;
    public float minContrast = 0f;
    public float maxContrast = 100f;

    public Slider lightSlider;
    public Slider contrastSlider;

    public CanvasGroup lightGroup;
    public CanvasGroup contrastGroup;

    public GameObject[] tutorialPanels;
    public GameObject introVideo;

    public float uiDisplayTime = 1f;
    private Coroutine lightRoutine;
    private Coroutine contrastRoutine;

    private void Awake()
    {
        Invoke("EndIntroVideo", 16f);
        Invoke("FadeOut", 15f);
        if (volume.profile.TryGet(out colorAdjustments)) { }
    }

    public void ApplyRotation(float horizontal, float vertical)
    {
        if (horizontalPivot == null || verticalPivot == null) return;

        // Rotación horizontal (WORLD)
        horizontalPivot.rotation = Quaternion.Euler(0f, horizontal, 0f);

        // Rotación vertical (LOCAL)
        verticalPivot.localRotation = Quaternion.Euler(vertical, 0f, 0f);
    }

    public void ApplyZoom(float zoom)
    {
        if (targetCamera != null)
        {
            targetCamera.localPosition = new Vector3(0, 0, -zoom);
        }
    }

    public void ApplyLight(float light)
    {
        if (Spotlight != null)
        { 
            Spotlight.intensity = light;
        }

    }

    public void ApplyContrast(float contrast)
    {
        if (colorAdjustments != null)
        {
            colorAdjustments.contrast.value = contrast;
        }
    }

    public void ShowLightUI(float value)
    {
        float normalized = Mathf.InverseLerp(minLight, maxLight, value);
        lightSlider.value = normalized;

        RestartCoroutine(ref lightRoutine, lightGroup);
    }

    public void ShowContrastUI(float value)
    {
        float normalized = Mathf.InverseLerp(minContrast, maxContrast, value);
        contrastSlider.value = normalized;

        RestartCoroutine(ref contrastRoutine, contrastGroup);
    }

    private IEnumerator ShowTemporary(CanvasGroup group)
    {
        group.gameObject.SetActive(true);
        group.alpha = 1;

        yield return new WaitForSeconds(uiDisplayTime);

        group.alpha = 0;
        group.gameObject.SetActive(false);
    }

    public void ShowMode(ControlMode mode)
    {
        Debug.Log("Modo actual: " + mode);
    }

    private void RestartCoroutine(ref Coroutine routine, CanvasGroup group)
    {
        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(ShowTemporary(group));
    }

    public void ShowPanel(int index)
    {
        for (int i = 0; i < tutorialPanels.Length; i++)
        {
            tutorialPanels[i].SetActive(i == index);
        }
    }

    public void HideAllPanels()
    {
        foreach (var panel in tutorialPanels)
        {
            panel.SetActive(false);
        }
    }

    public void EndIntroVideo()
    {
        introVideo.SetActive(false);
    }

    public RawImage video; // rawImage donde se reproduce el video
    public float duration = 1f;

    public void FadeOut()
    {
        StartCoroutine(Fade(1f, 0f));
    }

    public void FadeIn()
    {
        StartCoroutine(Fade(0f, 1f));
    }

    IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float time = 0f;
        Color color = video.color;

        while (time < duration)
        {
            float alpha = Mathf.Lerp(startAlpha, endAlpha, time / duration);
            video.color = new Color(color.r, color.g, color.b, alpha);

            time += Time.deltaTime;
            yield return null;
        }

        video.color = new Color(color.r, color.g, color.b, endAlpha);
    }
}