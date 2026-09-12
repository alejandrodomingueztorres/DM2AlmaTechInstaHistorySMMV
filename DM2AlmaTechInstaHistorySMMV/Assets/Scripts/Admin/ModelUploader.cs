using UnityEngine;
using System.IO;

public class ModelUploader : MonoBehaviour
{
    public string selectedFilePath;

    public void SelectModel()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        string path = UnityEditor.EditorUtility.OpenFilePanel(
            "Seleccionar modelo 3D",
            "",
            "obj,glb"
        );

        if (!string.IsNullOrEmpty(path))
        {
            selectedFilePath = path;
            Debug.Log("Archivo seleccionado: " + path);
        }
#endif
    }
}