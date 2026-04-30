using System.IO;
using UnityEngine;

public class RuntimeModelLoader : MonoBehaviour
{
    [Header("Modelo original de la escena")]
    [SerializeField] private GameObject defaultModelRoot;

    [Header("Contenedor del modelo actualizado")]
    [SerializeField] private Transform updatedModelContainer;

    [Header("Auto cargar al iniciar")]
    [SerializeField] private bool loadOnStart = true;

    private string CurrentPath => Path.Combine(Application.persistentDataPath, "AdminContent", "Models", "Current");

    private void Start()
    {
        if (loadOnStart)
            LoadCurrentModel();
    }

    public void LoadCurrentModel()
    {
        ClearUpdatedContainer();

        string objPath = Path.Combine(CurrentPath, "current_model.obj");
        string glbPath = Path.Combine(CurrentPath, "current_model.glb");

        if (File.Exists(objPath))
        {
            if (defaultModelRoot != null)
                defaultModelRoot.SetActive(false);

            GameObject loadedObj = SimpleOBJRuntimeLoader.LoadOBJ(objPath);
            loadedObj.name = "Modelo_OBJ_Actualizado";
            loadedObj.transform.SetParent(updatedModelContainer, false);
            loadedObj.transform.localPosition = Vector3.zero;
            loadedObj.transform.localRotation = Quaternion.identity;
            loadedObj.transform.localScale = Vector3.one;

            Debug.Log("Modelo OBJ actualizado cargado: " + objPath);
            return;
        }

        if (File.Exists(glbPath))
        {
            if (defaultModelRoot != null)
                defaultModelRoot.SetActive(false);

            GameObject placeholder = GameObject.CreatePrimitive(PrimitiveType.Cube);
            placeholder.name = "GLB_detectado_pendiente_loader";
            placeholder.transform.SetParent(updatedModelContainer, false);
            placeholder.transform.localPosition = Vector3.zero;
            placeholder.transform.localScale = Vector3.one * 0.5f;

            Debug.LogWarning("GLB detectado, pero falta integrar glTFast para visualizarlo realmente: " + glbPath);
            return;
        }

        if (defaultModelRoot != null)
            defaultModelRoot.SetActive(true);

        Debug.Log("No hay modelo actualizado. Se usa la urna original.");
    }

    private void ClearUpdatedContainer()
    {
        if (updatedModelContainer == null)
        {
            Debug.LogError("No se asignó UpdatedModelContainer.");
            return;
        }

        for (int i = updatedModelContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(updatedModelContainer.GetChild(i).gameObject);
        }
    }
}