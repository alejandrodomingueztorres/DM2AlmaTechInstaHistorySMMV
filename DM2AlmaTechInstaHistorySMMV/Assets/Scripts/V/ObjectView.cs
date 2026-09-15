using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.UI;
using System.Collections;
using static ObjectModel;
using System.IO;


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

    [Header("Intercambio de modelo 3D")]
    [Tooltip("Modelo original de la escena. Se muestra cuando no hay un modelo cargado desde disco.")]
    public GameObject defaultModelRoot;
    [Tooltip("Contenedor donde se instancia el modelo cargado en runtime.")]
    public Transform updatedModelContainer;

    private string CurrentModelPath =>
        Path.Combine(Application.persistentDataPath, "AdminContent", "Models", "Current", "current_model.obj");

    // Tamaño (en metros, la dimensión más grande de su caja envolvente) de la
    // pieza original. Se usa como referencia para reescalar cualquier modelo
    // que se cargue después, sin importar en qué unidades fue exportado.
    private float defaultModelMaxDimension = 1f;

    private void Awake()
    {
        Invoke("EndIntroVideo", 16f);
        Invoke("FadeOut", 15f);
        if (volume.profile.TryGet(out colorAdjustments)) { }
    }

    public void ApplyRotation(float horizontal, float vertical)
    {
        if (horizontalPivot == null || verticalPivot == null) return;

        // Rotaci�n horizontal (WORLD)
        horizontalPivot.rotation = Quaternion.Euler(0f, horizontal, 0f);

        // Rotaci�n vertical (LOCAL)
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

    /// <summary>
    /// Muestra el modelo actualizado si existe en disco (current_model.obj),
    /// o el modelo original de la escena si no hay ninguno cargado.
    /// Reemplaza a RuntimeModelLoader.cs y RuntimeModelAutoLoad.cs.
    /// </summary>
    public void LoadCurrentModel()
    {
        if (updatedModelContainer == null)
        {
            Debug.LogError("No se asignó Updated Model Container en ObjectView.");
            return;
        }

        // Se mide acá (y no en Awake) para no depender de si ObjectView.Awake()
        // corrió antes o después de quien llama a este método.
        if (defaultModelRoot != null)
        {
            float maxDim = GetMaxDimension(defaultModelRoot);
            if (maxDim > 0f)
                defaultModelMaxDimension = maxDim;
        }
        else
        {
            Debug.LogWarning("Default Model Root no está asignado en ObjectView: los modelos nuevos se reescalan contra 1 metro por defecto.");
        }

        for (int i = updatedModelContainer.childCount - 1; i >= 0; i--)
            Destroy(updatedModelContainer.GetChild(i).gameObject);

        if (File.Exists(CurrentModelPath))
        {
            if (defaultModelRoot != null)
                defaultModelRoot.SetActive(false);

            GameObject loadedObj = Simpleobjruntimeloader.LoadOBJ(CurrentModelPath);
            loadedObj.name = "Modelo_Actualizado";
            loadedObj.transform.SetParent(updatedModelContainer, false);
            loadedObj.transform.localPosition = Vector3.zero;
            loadedObj.transform.localRotation = Quaternion.identity;
            loadedObj.transform.localScale = Vector3.one;

            NormalizeScale(loadedObj);

            Debug.Log("Modelo actualizado cargado: " + CurrentModelPath);
        }
        else
        {
            if (defaultModelRoot != null)
                defaultModelRoot.SetActive(true);

            Debug.Log("No hay modelo actualizado. Se muestra el modelo original.");
        }
    }

    /// <summary>
    /// Reescala el modelo recién cargado para que su dimensión más grande
    /// coincida con la de la pieza original, sin importar en qué unidades
    /// (cm, mm, m) haya sido exportado el archivo .obj.
    /// </summary>
    private void NormalizeScale(GameObject go)
    {
        float maxDim = GetMaxDimension(go);
        if (maxDim <= 0f) return;

        float scaleFactor = defaultModelMaxDimension / maxDim;
        go.transform.localScale = Vector3.one * scaleFactor;
    }

    private float GetMaxDimension(GameObject go)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) return 0f;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        return Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
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