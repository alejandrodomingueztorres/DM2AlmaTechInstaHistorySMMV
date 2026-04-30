using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ContentPreviewManager : MonoBehaviour
{
    [Header("Audio Preview")]
    [SerializeField] private AudioSource previewAudioSource;

    [Header("SRT / Narration Preview")]
    [SerializeField] private GameObject textPreviewPopup;
    [SerializeField] private TMP_Text textPreview;

    [Header("Model Preview")]
    [SerializeField] private Transform modelPreviewContainer;
    [SerializeField] private GameObject defaultModelPreview;
    [SerializeField] private float previewTargetSize = 2.2f;
    [SerializeField] private Vector3 previewLocalRotation = new Vector3(0, 180, 0);

    [Header("Feedback")]
    [SerializeField] private TMP_Text feedbackText;

    private string selectedAudioPath;
    private string selectedTextPath;
    private string selectedModelPath;

    public void SelectAudioForPreview()
    {
#if UNITY_EDITOR
        string path = EditorUtility.OpenFilePanel("Seleccionar audio para vista previa", "", "wav,mp3");
        if (!string.IsNullOrEmpty(path))
        {
            selectedAudioPath = path;
            ShowFeedback("Audio seleccionado: " + Path.GetFileName(path), true);
        }
#else
        ShowFeedback("Selector runtime pendiente para build.", false);
#endif
    }

    public void PreviewSelectedAudio()
    {
        if (string.IsNullOrEmpty(selectedAudioPath) || !File.Exists(selectedAudioPath))
        {
            ShowFeedback("Primero selecciona un audio válido.", false);
            return;
        }

        string extension = Path.GetExtension(selectedAudioPath).ToLower();

        if (extension != ".wav" && extension != ".mp3")
        {
            ShowFeedback("Formato de audio no permitido. Usa .wav o .mp3.", false);
            return;
        }

        StartCoroutine(LoadAndPlayAudio(selectedAudioPath, extension));
    }

    private IEnumerator LoadAndPlayAudio(string path, string extension)
    {
        string url = "file:///" + path.Replace("\\", "/");
        AudioType audioType = extension == ".mp3" ? AudioType.MPEG : AudioType.WAV;

        using UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(url, audioType);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            ShowFeedback("No se pudo cargar el audio.", false);
            Debug.LogError(request.error);
            yield break;
        }

        AudioClip clip = DownloadHandlerAudioClip.GetContent(request);

        if (previewAudioSource == null)
        {
            ShowFeedback("No hay AudioSource asignado.", false);
            yield break;
        }

        previewAudioSource.Stop();
        previewAudioSource.clip = clip;
        previewAudioSource.Play();

        ShowFeedback("Reproduciendo vista previa de audio.", true);
    }

    public void StopAudioPreview()
    {
        if (previewAudioSource == null)
        {
            ShowFeedback("No hay AudioSource asignado.", false);
            return;
        }

        previewAudioSource.Stop();
        ShowFeedback("Audio detenido.", true);
    }

    public void SelectTextForPreview()
    {
#if UNITY_EDITOR
        string path = EditorUtility.OpenFilePanel("Seleccionar archivo SRT", "", "srt");
        if (!string.IsNullOrEmpty(path))
        {
            selectedTextPath = path;
            ShowFeedback("SRT seleccionado: " + Path.GetFileName(path), true);
        }
#else
        ShowFeedback("Selector runtime pendiente para build.", false);
#endif
    }

    public void PreviewSelectedText()
    {
        if (string.IsNullOrEmpty(selectedTextPath) || !File.Exists(selectedTextPath))
        {
            ShowFeedback("Primero selecciona un archivo SRT válido.", false);
            return;
        }

        string extension = Path.GetExtension(selectedTextPath).ToLower();

        if (extension != ".srt")
        {
            ShowFeedback("Formato no permitido. Solo .srt.", false);
            return;
        }

        string content = File.ReadAllText(selectedTextPath);

        if (textPreview != null)
            textPreview.text = content;

        if (textPreviewPopup != null)
            textPreviewPopup.SetActive(true);

        ShowFeedback("Vista previa de SRT cargada.", true);
    }

    public void CloseTextPreviewPopup()
    {
        if (textPreviewPopup != null)
            textPreviewPopup.SetActive(false);

        ShowFeedback("Vista previa de SRT cerrada.", true);
    }

    public void SelectModelForPreview()
    {
#if UNITY_EDITOR
        string path = EditorUtility.OpenFilePanel("Seleccionar modelo 3D", "", "obj,glb");
        if (!string.IsNullOrEmpty(path))
        {
            selectedModelPath = path;
            ShowFeedback("Modelo seleccionado: " + Path.GetFileName(path), true);
        }
#else
        ShowFeedback("Selector runtime pendiente para build.", false);
#endif
    }

    public void PreviewSelectedModel()
    {
        if (string.IsNullOrEmpty(selectedModelPath) || !File.Exists(selectedModelPath))
        {
            ShowFeedback("Primero selecciona un modelo válido.", false);
            return;
        }

        string extension = Path.GetExtension(selectedModelPath).ToLower();

        if (extension != ".obj" && extension != ".glb")
        {
            ShowFeedback("Formato no permitido. Usa .obj o .glb.", false);
            return;
        }

        ClearModelPreview();

        if (extension == ".obj")
        {
            GameObject model = SimpleOBJRuntimeLoader.LoadOBJ(selectedModelPath);
            model.name = "Preview_OBJ_Model";
            model.transform.SetParent(modelPreviewContainer, false);
            PrepareModelForPreview(model);

            ShowFeedback("Vista previa de modelo OBJ cargada.", true);
        }
        else
        {
            GameObject placeholder = GameObject.CreatePrimitive(PrimitiveType.Cube);
            placeholder.name = "Preview_GLB_Pendiente_Loader";
            placeholder.transform.SetParent(modelPreviewContainer, false);
            PrepareModelForPreview(placeholder);

            ShowFeedback("GLB detectado. Vista previa real requiere loader GLB.", true);
        }
    }

    public void PreviewDefaultModel()
    {
        ClearModelPreview();

        if (defaultModelPreview == null)
        {
            ShowFeedback("No hay modelo por defecto asignado.", false);
            return;
        }

        GameObject model = Instantiate(defaultModelPreview, modelPreviewContainer);
        model.name = "Preview_Modelo_Por_Defecto";
        model.SetActive(true);

        PrepareModelForPreview(model);
        ShowFeedback("Vista previa del modelo por defecto cargada.", true);
    }

    public void ClearModelPreview()
    {
        if (modelPreviewContainer == null)
        {
            ShowFeedback("No hay contenedor de modelo asignado.", false);
            return;
        }

        for (int i = modelPreviewContainer.childCount - 1; i >= 0; i--)
            Destroy(modelPreviewContainer.GetChild(i).gameObject);
    }

    private void PrepareModelForPreview(GameObject model)
    {
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.Euler(previewLocalRotation);
        model.transform.localScale = Vector3.one;

        AssignFallbackMaterial(model);
        FitModelToPreviewSize(model, previewTargetSize);
    }

    private void FitModelToPreviewSize(GameObject model, float targetSize)
    {
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        Bounds bounds = renderers[0].bounds;

        foreach (Renderer renderer in renderers)
            bounds.Encapsulate(renderer.bounds);

        float maxSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        if (maxSize <= 0.0001f) return;

        float scaleFactor = targetSize / maxSize;
        model.transform.localScale *= scaleFactor;

        renderers = model.GetComponentsInChildren<Renderer>();
        bounds = renderers[0].bounds;

        foreach (Renderer renderer in renderers)
            bounds.Encapsulate(renderer.bounds);

        Vector3 offset = model.transform.position - bounds.center;
        model.transform.position += offset;
    }

    private void AssignFallbackMaterial(GameObject model)
    {
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>();

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
            shader = Shader.Find("Standard");

        Material fallbackMaterial = new Material(shader);
        fallbackMaterial.color = Color.white;

        foreach (Renderer renderer in renderers)
        {
            if (renderer.sharedMaterial == null)
                renderer.material = fallbackMaterial;
        }
    }

    private void ShowFeedback(string message, bool success)
    {
        Debug.Log(message);

        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = success ? Color.green : Color.red;
        }
    }
}