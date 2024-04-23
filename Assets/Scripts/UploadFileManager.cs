using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class UploadFileManager : MonoBehaviour
{
    private ShowFilesManager _showFilesManager;
    private string _finalPath;
    private readonly string _filteredFileType = NativeFilePicker.ConvertExtensionToFileType("jpg, png");
    private const string saveFilesPath = "Assets/Resources/Screens";

    private void Start()
    {
        _showFilesManager = FindObjectOfType<ShowFilesManager>();
    }

    public void OpenFileBrowser()
    {
        if (NativeFilePicker.IsFilePickerBusy())
            return;

        NativeFilePicker.Permission permission = NativeFilePicker.PickFile((path) =>
        {
            if (path == null)
                Debug.Log("Operation cancelled");
            else
            {
                Debug.Log("Picked file: " + path);
                _finalPath = path;
                StartCoroutine(LoadImage());
            }
        }, new string[] { _filteredFileType });

        Debug.Log("Permission result: " + permission);
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
        }
    }

    private void SaveToSO(Texture texture)
    {
        long time = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();

        string texturePath = $"{saveFilesPath}/texture-{time}.asset";
        string soPath = $"{saveFilesPath}/filename-{time}.asset";
        AssetDatabase.CreateAsset(texture, texturePath);

        ScreenDataSo screenDataSo = ScriptableObject.CreateInstance<ScreenDataSo>();
        screenDataSo.texture = texture;
        screenDataSo.texturePath = texturePath;
        screenDataSo.thisObjectPath = soPath;

        AssetDatabase.CreateAsset(screenDataSo, soPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        
        _showFilesManager.DisplayFile(screenDataSo);
    }
}