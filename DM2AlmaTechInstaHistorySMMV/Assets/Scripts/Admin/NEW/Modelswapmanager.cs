using TMPro;
using UnityEditor;
using UnityEngine;
using System.IO;


#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Controla el reemplazo del modelo 3D mostrado en la escena de visualización.
/// funcionar (solo mostraba un cubo de reemplazo), así que se quitó para no
/// dar una falsa sensación de funcionalidad. Si en algún momento se integra
/// un importador runtime de glTF (ej. glTFast), se puede sumar aquí y en
/// ObjectView.LoadCurrentModel() sin tocar el resto del sistema.
/// </summary>
public class ModelSwapManager : MonoBehaviour
{
    [Header("UI (opcional)")]
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private TMP_InputField runtimePathInput;

    [Header("Vista previa opcional")]
    [Tooltip("Solo se usa si esta escena tiene también un ObjectView visible (ej. panel de administración con preview en vivo).")]
    [SerializeField] private ObjectView previewView;

    private string ModelsFolder => Path.Combine(Application.persistentDataPath, "AdminContent", "Models");
    private string CurrentFolder => Path.Combine(ModelsFolder, "Current");
    private string PreviousFolder => Path.Combine(ModelsFolder, "Previous");
    private string CurrentModelPath => Path.Combine(CurrentFolder, "current_model.obj");

    private void Awake()
    {
        Directory.CreateDirectory(CurrentFolder);
        Directory.CreateDirectory(PreviousFolder);
    }

    /// <summary>Botón "Seleccionar archivo" (solo Editor / escritorio con Editor de Unity).</summary>
    public void SelectAndApplyModel()
    {
#if UNITY_EDITOR
        string path = EditorUtility.OpenFilePanel("Seleccionar modelo 3D (.obj)", "", "obj");
        if (!string.IsNullOrEmpty(path))
            ApplyModel(path);
#else
        ShowFeedback("En build, usa el campo de ruta.", false);
#endif
    }

    /// <summary>Botón "Aplicar" cuando el usuario escribe/pega una ruta en runtime.</summary>
    public void ApplyModelFromInputPath()
    {
        if (runtimePathInput == null)
        {
            ShowFeedback("No hay campo de ruta asignado.", false);
            return;
        }

        ApplyModel(runtimePathInput.text.Trim());
    }

    /// <summary>
    /// Aplica un modelo directamente: valida, respalda el Current actual (si
    /// existe) y copia el .obj + su .mtl + su textura, todos juntos.
    /// </summary>
    public void ApplyModel(string sourcePath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
        {
            ShowFeedback("El archivo seleccionado no existe.", false);
            return;
        }

        if (Path.GetExtension(sourcePath).ToLower() != ".obj")
        {
            ShowFeedback("Formato no permitido. Solo se acepta .obj.", false);
            return;
        }

        Directory.CreateDirectory(CurrentFolder);
        Directory.CreateDirectory(PreviousFolder);

        // Respaldamos TODO el contenido actual (obj + mtl + textura) antes de reemplazarlo.
        ClearFolder(PreviousFolder);
        if (HasFiles(CurrentFolder))
            CopyFolderContents(CurrentFolder, PreviousFolder);

        ClearFolder(CurrentFolder);

        File.Copy(sourcePath, CurrentModelPath, true);
        CopyCompanionFiles(sourcePath, CurrentFolder);

        ShowFeedback("Modelo actualizado correctamente.", true);

        if (previewView != null)
            previewView.LoadCurrentModel();
    }

    /// <summary>
    /// Deshace el último cambio. Si había un respaldo, lo restaura completo
    /// (obj + mtl + textura). Si no había respaldo (el cambio actual fue el
    /// primero), vuelve directamente al modelo original de la escena.
    /// </summary>
    public void UndoLastChange()
    {
        if (HasFiles(PreviousFolder))
        {
            ClearFolder(CurrentFolder);
            CopyFolderContents(PreviousFolder, CurrentFolder);
            ClearFolder(PreviousFolder);
            ShowFeedback("Se restauró el modelo anterior.", true);
        }
        else
        {
            ClearFolder(CurrentFolder);
            ShowFeedback("Se restauró el modelo original de la escena.", true);
        }

        if (previewView != null)
            previewView.LoadCurrentModel();
    }

    /// <summary>Reset total: vuelve al modelo original y borra cualquier respaldo. No se puede deshacer.</summary>
    public void ResetToDefaultModel()
    {
        ClearFolder(CurrentFolder);
        ClearFolder(PreviousFolder);

        ShowFeedback("Se restableció el modelo original de la escena.", true);

        if (previewView != null)
            previewView.LoadCurrentModel();
    }

    /// <summary>Copia el .mtl que el .obj referencia (mtllib) y la textura que ese .mtl referencia (map_Kd).</summary>
    private void CopyCompanionFiles(string objSourcePath, string destFolder)
    {
        string sourceDir = Path.GetDirectoryName(objSourcePath);
        string mtlFileName = Simpleobjruntimeloader.GetMaterialLibFileName(objSourcePath);

        if (string.IsNullOrEmpty(mtlFileName))
            return; // El modelo no tiene material/textura asociada, no hay nada más que copiar.

        string sourceMtlPath = Path.Combine(sourceDir, mtlFileName);
        if (!File.Exists(sourceMtlPath))
        {
            Debug.LogWarning("El .obj referencia un .mtl que no está junto a él: " + sourceMtlPath);
            return;
        }

        File.Copy(sourceMtlPath, Path.Combine(destFolder, mtlFileName), true);

        string textureFileName = Simpleobjruntimeloader.GetFirstTextureFileName(objSourcePath, mtlFileName);
        if (string.IsNullOrEmpty(textureFileName))
            return;

        string sourceTexturePath = Path.Combine(sourceDir, textureFileName);
        if (File.Exists(sourceTexturePath))
            File.Copy(sourceTexturePath, Path.Combine(destFolder, textureFileName), true);
        else
            Debug.LogWarning("El .mtl referencia una textura que no está junto al modelo: " + sourceTexturePath);
    }

    private bool HasFiles(string folder) => Directory.Exists(folder) && Directory.GetFiles(folder).Length > 0;

    private void ClearFolder(string folder)
    {
        Directory.CreateDirectory(folder);
        foreach (string file in Directory.GetFiles(folder))
            File.Delete(file);
    }

    private void CopyFolderContents(string sourceFolder, string destFolder)
    {
        Directory.CreateDirectory(destFolder);
        foreach (string file in Directory.GetFiles(sourceFolder))
            File.Copy(file, Path.Combine(destFolder, Path.GetFileName(file)), true);
    }

    private void ShowFeedback(string message, bool success)
    {
        Debug.Log(message);

        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = success ? Color.green : Color.red;
        }
    }
}