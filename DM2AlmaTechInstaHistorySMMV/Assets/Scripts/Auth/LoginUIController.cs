using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginUIController : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_Text feedbackText;

    [Header("Escena protegida")]
    [SerializeField] private string reportsSceneName = "AdminReportsScene";

    public void AttemptLogin()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowFeedback("Por favor, completa todos los campos.", false);
            return;
        }

        if (AuthManager.Instance == null)
        {
            ShowFeedback("Error interno: no se encontró el gestor de autenticación.", false);
            return;
        }

        bool success = AuthManager.Instance.ValidateCredentials(username, password);

        if (success)
        {
            ShowFeedback("Acceso permitido. Redirigiendo...", true);
            StartCoroutine(LoadReportsSceneWithDelay(3f));
        }
        else
        {
            ShowFeedback("Acceso denegado. Usuario o contraseña incorrectos.", false);
        }
    }

    private void ShowFeedback(string message, bool isSuccess)
    {
        feedbackText.text = message;
        feedbackText.color = isSuccess ? Color.green : Color.red;
    }

    private System.Collections.IEnumerator LoadReportsSceneWithDelay(float delay)
{
    yield return new WaitForSeconds(delay);
    SceneManager.LoadScene(reportsSceneName);
}
}