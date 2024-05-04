using System.Collections;
using Analysis;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Utils;

public class UploadFileManager : BaseManager
{
    private string _finalPath;
    private readonly string _filteredFileType = NativeFilePicker.ConvertExtensionToFileType("jpg, png");


    public async UniTask<(bool, ScreenDataSo)> SaveFile()
    {
        (bool, ScreenDataSo ) savedFile = (false, null);
        if (NativeFilePicker.IsFilePickerBusy())
            return savedFile;

        bool isLoaded = false;
        NativeFilePicker.Permission permission = NativeFilePicker.PickFile(async (path) =>
        {
            if (path == null)
            {
                Debug.Log("Operation cancelled");
            }
            else
            {
                Debug.Log("Picked file: " + path);
                _finalPath = path;
                savedFile.Item2 = await LoadImage();
                savedFile.Item1 = true;
            }
            isLoaded = true;
        }, new string[] { _filteredFileType });

        UniTask.WaitUntil(() => isLoaded);
        Debug.Log("Permission result: " + permission);
        return savedFile;
    }

    private async UniTask<ScreenDataSo> LoadImage()
    {
        var www = UnityWebRequestTexture.GetTexture(_finalPath);
        await www.SendWebRequest();
        ScreenDataSo screenDataSo = null;
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Texture texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            screenDataSo = ScreenDataSo.CreateScreenDataSo((Texture2D)texture);
            EditorUtility.FocusProjectWindow();
        }

        return screenDataSo;
    }
}