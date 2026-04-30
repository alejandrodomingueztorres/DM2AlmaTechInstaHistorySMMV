using System.IO;
using TMPro;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ModelUploadManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private TMP_InputField runtimePathInput;

    [Header("Vista previa opcional")]
    [SerializeField] private RuntimeModelLoader previewLoader;

    private string selectedLibraryModelPath;

    private string RootPath => Path.Combine(Application.persistentDataPath, "AdminContent", "Models");
    private string LibraryPath => Path.Combine(RootPath, "Library");
    private string CurrentPath => Path.Combine(RootPath, "Current");
    private string BackupPath => Path.Combine(RootPath, "Backup");

    private void Awake()
    {
        CreateFolders();
    }

    public void SelectAndUploadModel()
    {
#if UNITY_EDITOR
        string path = EditorUtility.OpenFilePanel("Seleccionar modelo 3D", "", "obj,glb");

        if (!string.IsNullOrEmpty(path))
        {
            UploadModelToLibrary(path);
        }
#else
        ShowFeedback("En build usa el campo de ruta o un file picker runtime.", false);
#endif
    }

    public void UploadModelFromInputPath()
    {
        if (runtimePathInput == null)
        {
            ShowFeedback("No hay campo de ruta asignado.", false);
            return;
        }

        UploadModelToLibrary(runtimePathInput.text.Trim());
    }

    public void UploadModelToLibrary(string sourcePath)
    {
        CreateFolders();

        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            ShowFeedback("No se seleccionó ningún archivo.", false);
            return;
        }

        if (!File.Exists(sourcePath))
        {
            ShowFeedback("El archivo seleccionado no existe.", false);
            return;
        }

        string extension = Path.GetExtension(sourcePath).ToLower();

        if (extension != ".obj" && extension != ".glb")
        {
            ShowFeedback("Formato no permitido. Solo .obj y .glb.", false);
            return;
        }

        string fileName = Path.GetFileName(sourcePath);
        string destinationPath = Path.Combine(LibraryPath, fileName);

        File.Copy(sourcePath, destinationPath, true);
        selectedLibraryModelPath = destinationPath;

        ShowFeedback("Modelo subido a Library: " + fileName, true);
        Debug.Log("Modelo guardado en Library: " + destinationPath);
    }

    public void ApplySelectedModel()
    {
        if (string.IsNullOrEmpty(selectedLibraryModelPath) || !File.Exists(selectedLibraryModelPath))
        {
            ShowFeedback("Primero debes subir/seleccionar un modelo válido.", false);
            return;
        }

        CreateFolders();

        string extension = Path.GetExtension(selectedLibraryModelPath).ToLower();

        BackupCurrentModelOrDefault();

        ClearFolder(CurrentPath);

        string currentModelPath = Path.Combine(CurrentPath, "current_model" + extension);
        File.Copy(selectedLibraryModelPath, currentModelPath, true);

        File.WriteAllText(Path.Combine(CurrentPath, "model_info.txt"), currentModelPath);

        ShowFeedback("Modelo actualizado correctamente.", true);
        Debug.Log("Modelo actual: " + currentModelPath);

        if (previewLoader != null)
            previewLoader.LoadCurrentModel();
    }

    public void RestorePreviousModel()
    {
        CreateFolders();

        string defaultFlag = Path.Combine(BackupPath, "restore_default.flag");
        string backupObj = Path.Combine(BackupPath, "backup_model.obj");
        string backupGlb = Path.Combine(BackupPath, "backup_model.glb");

        ClearFolder(CurrentPath);

        if (File.Exists(defaultFlag))
        {
            ClearFolder(BackupPath);
            ShowFeedback("Se restauró el modelo original por defecto.", true);

            if (previewLoader != null)
                previewLoader.LoadCurrentModel();

            return;
        }

        if (File.Exists(backupObj))
        {
            string restoredPath = Path.Combine(CurrentPath, "current_model.obj");
            File.Copy(backupObj, restoredPath, true);
            File.WriteAllText(Path.Combine(CurrentPath, "model_info.txt"), restoredPath);
        }
        else if (File.Exists(backupGlb))
        {
            string restoredPath = Path.Combine(CurrentPath, "current_model.glb");
            File.Copy(backupGlb, restoredPath, true);
            File.WriteAllText(Path.Combine(CurrentPath, "model_info.txt"), restoredPath);
        }
        else
        {
            ShowFeedback("No hay modelo anterior para restaurar.", false);
            return;
        }

        ClearFolder(BackupPath);

        ShowFeedback("Modelo anterior restaurado correctamente.", true);

        if (previewLoader != null)
            previewLoader.LoadCurrentModel();
    }

    private void BackupCurrentModelOrDefault()
    {
        ClearFolder(BackupPath);

        string currentObj = Path.Combine(CurrentPath, "current_model.obj");
        string currentGlb = Path.Combine(CurrentPath, "current_model.glb");

        if (File.Exists(currentObj))
        {
            File.Copy(currentObj, Path.Combine(BackupPath, "backup_model.obj"), true);
        }
        else if (File.Exists(currentGlb))
        {
            File.Copy(currentGlb, Path.Combine(BackupPath, "backup_model.glb"), true);
        }
        else
        {
            File.WriteAllText(Path.Combine(BackupPath, "restore_default.flag"), "restore_default");
        }
    }

    private void CreateFolders()
    {
        Directory.CreateDirectory(LibraryPath);
        Directory.CreateDirectory(CurrentPath);
        Directory.CreateDirectory(BackupPath);
    }

    private void ClearFolder(string folderPath)
    {
        if (!Directory.Exists(folderPath)) return;

        foreach (string file in Directory.GetFiles(folderPath))
            File.Delete(file);
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