using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class FileManager : MonoBehaviour
{
    public RawImage rawImage;

    private string _finalPath;


    private readonly string _filteredFileType = NativeFilePicker.ConvertExtensionToFileType("jpg, png");


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
            rawImage.texture = myTexture;
            SaveToSO(myTexture);
        }
    }

    private void SaveToSO(Texture texture)
    {
        long time = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
        
        string texturePath = $"Assets/Screens/texture-{time}.asset";
        AssetDatabase.CreateAsset(texture, texturePath);
        
        ScreenDataSo screenDataSo = ScriptableObject.CreateInstance<ScreenDataSo>();
        screenDataSo.finalPath = _finalPath;
        screenDataSo.myTexture = texture;

        string soPath = $"Assets/Screens/filename-{time}.asset";
        AssetDatabase.CreateAsset(screenDataSo, soPath);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
    }
}