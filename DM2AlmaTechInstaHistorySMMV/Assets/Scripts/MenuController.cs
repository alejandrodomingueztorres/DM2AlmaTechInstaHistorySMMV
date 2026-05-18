using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void IniciarExploracion()
    {
        SceneManager.LoadScene("3dview");
    }

    public void IrExploracion()
    {
        SceneManager.LoadScene("EscenaExploración");
    }

    public void IrPuzzle()
    {
        SceneManager.LoadScene("Puzzle");
    }
    public void IrRitual()
    {
        SceneManager.LoadScene("Reto");
    }
    public void IrAdmin()
    {
        SceneManager.LoadScene("AdminLoginScene");
    }

    public void IrMenu()
    {
        SceneManager.LoadScene("MenuPrincipal");
    }
}
