using UnityEngine;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance;

    [Header("Usuario administrativo de prueba")]
    [SerializeField] private string adminUsername = "admin";
    [SerializeField] private string adminPassword = "1234";

    public bool IsAuthenticated { get; private set; }
    public string CurrentUsername { get; private set; } = "";

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
        bool isValid = username == adminUsername && password == adminPassword;

        if (isValid)
        {
            IsAuthenticated = true;
            CurrentUsername = username;
            return true;
        }

        IsAuthenticated = false;
        CurrentUsername = "";
        return false;
    }

    public void Logout()
    {
        IsAuthenticated = false;
        CurrentUsername = "";
    }
}