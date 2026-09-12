using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class ReferenceManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject referencePanel;

    public TMP_Text textAutor;
    public TMP_Text textFuente;
    public TMP_Text textAnio;
    public Image qrImage;

    [Header("Referencias por escena")]
    public ReferenceData exploracion;
    public ReferenceData puzzle;
    public ReferenceData ritual;

    void Start()
    {
        LoadReference();
    }

    void LoadReference()
    {
        string scene = SceneManager.GetActiveScene().name;

        ReferenceData data = null;

        if (scene == "3dview")
            data = exploracion;
        else if (scene == "Puzzle")
            data = puzzle;
        else if (scene == "Reto")
            data = ritual;

        if (data != null)
        {
            textAutor.text = data.autor;
            textFuente.text = data.fuente;
            textAnio.text = data.anio;
            qrImage.sprite = data.qr;
        }
    }

    public void OpenReferences()
    {
        referencePanel.SetActive(true);
    }

    public void CloseReferences()
    {
        referencePanel.SetActive(false);
    }

    [System.Serializable]
    public class ReferenceData
    {
        public string autor;
        public string fuente;
        public string anio;
        public Sprite qr;
    }
}