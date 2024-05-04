using System.Collections;
using Analysis;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Utils;

public class UploadFileManager : BaseManager
{
    private string _finalPath;
    private readonly string _filteredFileType = NativeFilePicker.ConvertExtensionToFileType("jpg, png");


    public bool SaveFile()
    {
        if (NativeFilePicker.IsFilePickerBusy())
            return false;

        var savedFile = false;
        NativeFilePicker.Permission permission = NativeFilePicker.PickFile((path) =>
        {
            if (path == null)
            {
                Debug.Log("Operation cancelled");
            }
            else
            {
                Debug.Log("Picked file: " + path);
                _finalPath = path;
                StartCoroutine(LoadImage());
                savedFile = true;
            }
        }, new string[] { _filteredFileType });

        Debug.Log("Permission result: " + permission);
        return savedFile;
    }

    private IEnumerator LoadImage()
    {
        var www = UnityWebRequestTexture.GetTexture(_finalPath);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Texture myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            SaveToSO(myTexture);
            EditorUtility.FocusProjectWindow();
        }
    }

    private void SaveToSO(Texture texture)
    {
        var screenDataSo = ScreenDataSo.CreateScreenDataSo((Texture2D)texture);
        // _showFilesManager.DisplayFile(screenDataSo);
    }
}