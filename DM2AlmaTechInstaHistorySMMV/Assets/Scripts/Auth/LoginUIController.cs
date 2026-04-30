using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginUIController : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_Text feedbackText;

    [Header("Escenas por rol")]
    [SerializeField] private string adminSceneName = "AdminReportsScene";
    [SerializeField] private string institutionalSceneName = "InstitutionalScene";

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
            if (AuthManager.Instance.CurrentRole == "ADMIN")
            {
                ShowFeedback("Acceso administrativo permitido. Redirigiendo...", true);
                StartCoroutine(LoadSceneWithDelay(adminSceneName, 3f));
            }
            else if (AuthManager.Instance.CurrentRole == "INSTITUCIONAL")
            {
                ShowFeedback("Acceso institucional permitido. Redirigiendo...", true);
                StartCoroutine(LoadSceneWithDelay(institutionalSceneName, 3f));
            }
            else
            {
                ShowFeedback("Rol no reconocido. Contacta al administrador.", false);
            }
        }
        else
        {
            ShowFeedback("Acceso denegado. Usuario o contraseña incorrectos.", false);
        }
    }

    private void ShowFeedback(string message, bool isSuccess)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = isSuccess ? Color.green : Color.red;
        }

        Debug.Log("[Login] " + message);
    }

    private System.Collections.IEnumerator LoadSceneWithDelay(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }
}