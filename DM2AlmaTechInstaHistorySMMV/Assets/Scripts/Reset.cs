using UnityEngine;
using UnityEngine.SceneManagement;

public class Reset : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public bool restart;
    public bool cont;
    public float seconds;

    void Start()
    {
        if (restart==true)
        {
            Invoke("RestartExperience", seconds);
        }

        if (cont==true)
        {
            Invoke("LoadNext", seconds);
        }
        
    }

    // Update is called once per frame
    public void RestartExperience()
    {
        SceneManager.LoadScene("MenuPrincipal");
    }

    public void LoadNext()
    {
        SceneManager.LoadScene("MensajeRecon");
    }
}
