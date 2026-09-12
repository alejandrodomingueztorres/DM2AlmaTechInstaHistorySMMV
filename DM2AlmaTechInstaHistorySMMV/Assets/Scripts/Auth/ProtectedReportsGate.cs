using UnityEngine;
using UnityEngine.SceneManagement;

public class ProtectedReportsGate : MonoBehaviour
{
    [SerializeField] private string loginSceneName = "AdminLoginScene";

    private void Start()
    {
        if (AuthManager.Instance == null || !AuthManager.Instance.IsAuthenticated)
        {
            SceneManager.LoadScene(loginSceneName);
        }
    }
}