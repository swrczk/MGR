using Analysis;
using UnityEngine;
using Utils;

public class ShowFilesManager : BaseManager
{
    public void DisplayFiles(ScreenElement filePrefab, Transform parentTransform)
    {
        foreach (Transform child in parentTransform)
        {
            Destroy(child.gameObject);
        }

        ScreenDataSo[] screenDataArray = Resources.LoadAll<ScreenDataSo>("");
        foreach (ScreenDataSo file in screenDataArray)
        {
            DisplayFile(file, filePrefab, parentTransform);
        }
    }

    public void DisplayFile(ScreenDataSo screenDataSo, ScreenElement filePrefab, Transform parentTransform)
    {
        var fileObject = Instantiate(filePrefab, parentTransform);
        fileObject.Setup(screenDataSo);
    }
}