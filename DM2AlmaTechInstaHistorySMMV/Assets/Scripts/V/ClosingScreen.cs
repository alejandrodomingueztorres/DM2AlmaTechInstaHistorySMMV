using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class ClosingScreen : MonoBehaviour
{
    [Header("UI Referencias")]
    public GameObject closingPanel;
    public TextMeshProUGUI messageText;

    [Header("Configuración")]
    [TextArea]
    public string finalMessage = "Escanea y conecta con el alma viva de nuestra memoria ancestral";

    [Header("Animación de aparición")]
    public float fadeDuration = 1.5f;
    public float delayBeforeShow = 0.5f;

    [Header("Efecto de reconocimiento")]
    public RecognitionEffect recognitionEffect;

    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = closingPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = closingPanel.AddComponent<CanvasGroup>();

        // Inicia oculto
        canvasGroup.alpha = 0f;
        closingPanel.SetActive(false);
    }
    

    // Llama este método cuando la experiencia termine
    public void ShowClosingScreen()
    {
        if (recognitionEffect != null)
            recognitionEffect.TriggerEffect();

        messageText.text = finalMessage;
        closingPanel.SetActive(true);
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        yield return new WaitForSeconds(delayBeforeShow);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }
  
}