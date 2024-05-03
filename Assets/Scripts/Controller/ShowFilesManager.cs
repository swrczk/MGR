using UnityEngine;

public class ShowFilesManager : MonoBehaviour
{
    [SerializeField]
    private ScreenElement filePrefab;

    [SerializeField]
    private Transform content;

    private const string DataPath = "Screens";

    private void Start()
    {
        DisplayFiles();
    }

    public void DisplayFiles()
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        ScreenDataSo[] screenDataArray = Resources.LoadAll<ScreenDataSo>("");
        foreach (ScreenDataSo file in screenDataArray)
        {
            DisplayFile(file);
        }
    }

    public void DisplayFile(ScreenDataSo screenDataSo)
    {
        var fileObject = Instantiate(filePrefab, content);
        fileObject.Setup(screenDataSo);
    }
}