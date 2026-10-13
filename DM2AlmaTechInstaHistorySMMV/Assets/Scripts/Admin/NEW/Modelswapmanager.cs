using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Controla el reemplazo del modelo 3D mostrado en la escena de visualización.
///
/// Carpetas (dentro de Application.persistentDataPath/AdminContent/Models):
///   - Current/  -> current_model.obj (+ current_texture.png/.jpg si hay textura)
///   - Previous/ -> copia del Current anterior, para UndoLastChange()
///
/// Cómo se elige el archivo según la plataforma:
///   - Editor:  ventana de archivos de Unity. Si el .obj tiene .mtl, la textura se busca junto a él.
///   - Android: selector nativo del sistema (plugin Native File Picker). El usuario marca
///              el .obj y su textura (.png/.jpg) a la vez; el .mtl es opcional.
///   - Otros:   campo de ruta (runtimePathInput).
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

    /// <summary>Botón "Seleccionar archivo".</summary>
    public void SelectAndApplyModel()
    {
#if UNITY_EDITOR
        string path = EditorUtility.OpenFilePanel("Seleccionar modelo 3D (.obj)", "", "obj");
        if (!string.IsNullOrEmpty(path))
            ApplyModel(path);
#elif UNITY_ANDROID
        // "*/*": el .obj no tiene un tipo MIME fiable en Android, así que se permite cualquier archivo.
        NativeFilePicker.PickMultipleFiles(paths => OnFilesPicked(paths), "*/*");
#else
        ShowFeedback("En esta plataforma, usa el campo de ruta.", false);
#endif
    }

    /// <summary>Botón "Aplicar" cuando el usuario escribe/pega una ruta.</summary>
    public void ApplyModelFromInputPath()
    {
        if (runtimePathInput == null)
        {
            ShowFeedback("No hay campo de ruta asignado.", false);
            return;
        }

        ApplyModel(runtimePathInput.text.Trim());
    }

    /// <summary>Callback del selector de Android: recibe todos los archivos que el usuario marcó.</summary>
    private void OnFilesPicked(string[] paths)
    {
        if (paths == null || paths.Length == 0)
        {
            ShowFeedback("No se seleccionó ningún archivo. Marca el .obj y su textura (.png/.jpg) a la vez.", false);
            return;
        }

        string objPath = null;
        string mtlPath = null;
        List<string> images = new List<string>();

        foreach (string path in paths)
        {
            string ext = Path.GetExtension(path).ToLower();

            if (ext == ".obj") objPath = path;
            else if (ext == ".mtl") mtlPath = path;
            else if (IsSupportedImage(path)) images.Add(path);
        }

        if (objPath == null)
        {
            ShowFeedback("Entre los archivos seleccionados no hay ningún .obj.", false);
            return;
        }

        ApplyModel(objPath, ChooseTexture(images, mtlPath));
    }

    /// <summary>Aplica un .obj. La textura se busca junto a él usando su .mtl.</summary>
    public void ApplyModel(string objPath)
    {
        ApplyModel(objPath, FindSiblingTexture(objPath));
    }

    /// <summary>
    /// Valida, respalda el Current actual (si existe) y copia el modelo y su
    /// textura con nombres fijos (current_model.obj / current_texture.png|jpg).
    /// </summary>
    public void ApplyModel(string objPath, string texturePath)
    {
        if (string.IsNullOrWhiteSpace(objPath) || !File.Exists(objPath))
        {
            ShowFeedback("El archivo seleccionado no existe.", false);
            return;
        }

        if (Path.GetExtension(objPath).ToLower() != ".obj")
        {
            ShowFeedback("Formato no permitido. Solo se acepta .obj.", false);
            return;
        }

        Directory.CreateDirectory(CurrentFolder);
        Directory.CreateDirectory(PreviousFolder);

        // Respaldamos lo que hay en Current antes de reemplazarlo.
        ClearFolder(PreviousFolder);
        if (HasFiles(CurrentFolder))
            CopyFolderContents(CurrentFolder, PreviousFolder);

        ClearFolder(CurrentFolder);

        File.Copy(objPath, CurrentModelPath, true);

        bool hasTexture = !string.IsNullOrEmpty(texturePath) && File.Exists(texturePath);
        if (hasTexture && !IsSupportedImage(texturePath))
        {
            Debug.LogWarning("Formato de textura no soportado (usa .png o .jpg): " + texturePath);
            hasTexture = false;
        }

        if (hasTexture)
        {
            string ext = Path.GetExtension(texturePath).ToLower();
            File.Copy(texturePath, Path.Combine(CurrentFolder, "current_texture" + ext), true);
        }

        ShowFeedback(hasTexture ? "Modelo actualizado con textura." : "Modelo actualizado sin textura.", true);

        if (previewView != null)
            previewView.LoadCurrentModel();
    }

    /// <summary>
    /// Deshace el último cambio. Si había un respaldo, lo restaura.
    /// Si no lo había (el cambio actual fue el primero), vuelve al modelo original de la escena.
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

    // ---------- Búsqueda de textura ----------

    /// <summary>
    /// Entre las imágenes elegidas, usa la que el .mtl indica como textura difusa (map_Kd).
    /// Si no hay .mtl o no coincide ninguna, usa la primera imagen.
    /// </summary>
    private string ChooseTexture(List<string> images, string mtlPath)
    {
        if (images.Count == 0)
            return null;

        if (images.Count > 1 && mtlPath != null)
        {
            string wanted = GetTextureNameFromMtl(mtlPath);
            if (wanted != null)
            {
                foreach (string image in images)
                {
                    if (string.Equals(Path.GetFileName(image), wanted, System.StringComparison.OrdinalIgnoreCase))
                        return image;
                }
            }
        }

        return images[0];
    }

    /// <summary>Para Editor/PC: sigue obj -> mtllib -> map_Kd y devuelve la textura si está junto al .obj.</summary>
    private string FindSiblingTexture(string objPath)
    {
        if (string.IsNullOrWhiteSpace(objPath) || !File.Exists(objPath))
            return null;

        string dir = Path.GetDirectoryName(objPath);
        string mtlName = null;
        int linesRead = 0;

        // mtllib está siempre al principio del archivo: no hace falta leerlo entero.
        foreach (string rawLine in File.ReadLines(objPath))
        {
            string line = rawLine.Trim();

            if (line.StartsWith("mtllib "))
            {
                mtlName = line.Substring(7).Trim();
                break;
            }

            if (++linesRead > 100)
                break;
        }

        if (mtlName == null)
            return null;

        string mtlPath = Path.Combine(dir, mtlName);
        if (!File.Exists(mtlPath))
            return null;

        string textureName = GetTextureNameFromMtl(mtlPath);
        if (textureName == null)
            return null;

        string texturePath = Path.Combine(dir, textureName);
        return File.Exists(texturePath) ? texturePath : null;
    }

    /// <summary>Nombre de archivo (sin carpetas) de la primera textura difusa (map_Kd) de un .mtl.</summary>
    private string GetTextureNameFromMtl(string mtlPath)
    {
        foreach (string rawLine in File.ReadLines(mtlPath))
        {
            string line = rawLine.Trim();

            if (line.StartsWith("map_Kd "))
                return Path.GetFileName(line.Substring(7).Trim().Replace('\\', '/'));
        }

        return null;
    }

    private bool IsSupportedImage(string path)
    {
        string ext = Path.GetExtension(path).ToLower();
        return ext == ".png" || ext == ".jpg" || ext == ".jpeg";
    }

    // ---------- Utilidades de carpetas ----------

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
