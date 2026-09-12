using UnityEngine;
using UnityEngine.EventSystems;

public class BotonInicio : MonoBehaviour
{
    public GameObject btn;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EventSystem.current.SetSelectedGameObject(btn);
    }
}
