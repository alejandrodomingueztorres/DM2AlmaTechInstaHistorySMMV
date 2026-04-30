using UnityEngine;

public class RuntimeModelAutoLoad : MonoBehaviour
{
    [SerializeField] private RuntimeModelLoader loader;

    private void Start()
    {
        if (loader != null)
        {
            loader.LoadCurrentModel();
        }
    }
}