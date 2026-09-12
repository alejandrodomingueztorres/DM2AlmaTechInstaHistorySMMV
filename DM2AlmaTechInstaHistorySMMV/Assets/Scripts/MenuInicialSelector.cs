using UnityEngine;
using UnityEngine.EventSystems;

public class MenuInicialSelector : MonoBehaviour
{
    public GameObject botonInicial;
    public GameObject botonSecundario;

    void Start()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(botonInicial);
        EventSystem.current.SetSelectedGameObject(botonSecundario);
    }
}
