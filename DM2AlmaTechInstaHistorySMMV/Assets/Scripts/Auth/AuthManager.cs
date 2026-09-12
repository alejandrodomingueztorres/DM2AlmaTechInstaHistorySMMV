using UnityEngine;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance;

    [Header("Admin")]
    [SerializeField] private string adminUsername = "admin";
    [SerializeField] private string adminPassword = "1234";

    [Header("Usuario institucional de prueba")]
    [SerializeField] private string institutionalUsername = "institucional";
    [SerializeField] private string institutionalPassword = "1234";

    public bool IsAuthenticated { get; private set; }
    public string CurrentUsername { get; private set; } = "";
    public string CurrentRole { get; private set; } = ""; // 🔥 NUEVO

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool ValidateCredentials(string username, string password)
    {
        // ADMIN
        if (username == adminUsername && password == adminPassword)
        {
            IsAuthenticated = true;
            CurrentUsername = username;
            CurrentRole = "ADMIN";
            Debug.Log("Login como ADMIN");
            return true;
        }

        // INSTITUCIONAL
        if (username == institutionalUsername && password == institutionalPassword)
        {
            IsAuthenticated = true;
            CurrentUsername = username;
            CurrentRole = "INSTITUCIONAL";
            Debug.Log("Login como USUARIO INSTITUCIONAL");
            return true;
        }

        // INVALIDO
        IsAuthenticated = false;
        CurrentUsername = "";
        CurrentRole = "";
        return false;
    }

    public void Logout()
    {
        IsAuthenticated = false;
        CurrentUsername = "";
        CurrentRole = "";
    }
}