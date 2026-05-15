using UnityEngine;
using UnityEngine.SceneManagement;

public class Reset : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public bool restart;
    public float seconds;

    void Start()
    {
        if (restart==true)
        {
            Invoke("RestartExperience", seconds);
        }
        
    }

    // Update is called once per frame
    public void RestartExperience()
    {
        SceneManager.LoadScene("MenuPrincipal");
    }
}
