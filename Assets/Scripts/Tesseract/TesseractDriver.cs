using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class TesseractDriver
{
    private TesseractWrapper _tesseract;
    private static readonly List<string> fileNames = new List<string> { "tessdata.tgz" };

    public string CheckTessVersion()
    {
        _tesseract = new TesseractWrapper();

        try
        {
            var version = "Tesseract version: " + _tesseract.Version();
            Debug.Log(version);
            return version;
        }
        catch (Exception e)
        {
            var errorMessage = e.GetType() + " - " + e.Message;
            Debug.LogError("Tesseract version: " + errorMessage);
            return errorMessage;
        }
    }

    public async UniTask<bool> TryToSetup()
    {
#if UNITY_EDITOR
        return OcrSetup();
#elif UNITY_ANDROID
        return await CopyAllFilesToPersistentData(fileNames);
#else
        return OcrSetup();
#endif
    }

    public bool OcrSetup()
    {
        _tesseract = new TesseractWrapper();

#if UNITY_EDITOR
        var datapath = Path.Combine(Application.streamingAssetsPath, "tessdata");
#elif UNITY_ANDROID
        string datapath = Application.persistentDataPath + "/tessdata/";
#else
        string datapath = Path.Combine(Application.streamingAssetsPath, "tessdata");
#endif

        if (_tesseract.Init("eng", datapath))
        {
            Debug.Log("Init Successful");
            return true;
        }

        Debug.LogError(_tesseract.GetErrorMessage());
        return false;
    }

    private async UniTask<bool> CopyAllFilesToPersistentData(List<string> fileNames)
    {
        var fromPath = "jar:file://" + Application.dataPath + "!/assets/";
        var toPath = Application.persistentDataPath + "/";

        foreach (var fileName in fileNames)
        {
            if (!File.Exists(toPath + fileName))
            {
                Debug.Log($"Copying from {fromPath}{fileName} to {toPath}");
                using (var www = UnityWebRequest.Get(fromPath + fileName))
                {
                    // Send the request and await its completion
                    await www.SendWebRequest();

                    // Check for errors
                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError($"Error while downloading file: {www.error}");
                        continue; // Skip to the next file
                    }

                    // Write data to file
                    File.WriteAllBytes(toPath + fileName, www.downloadHandler.data);
                    Debug.Log("File copy done");
                }
            }
            else
            {
                Debug.Log($"File exists! {toPath}{fileName}");
            }
            
            UnZipData(fileName);
        }

        return OcrSetup();
    }

    public string GetErrorMessage()
    {
        return _tesseract?.GetErrorMessage();
    }

    public string Recognize(Texture2D imageToRecognize)
    {
        return _tesseract.Recognize(imageToRecognize);
    }

    public Texture2D GetHighlightedTexture()
    {
        return _tesseract.GetHighlightedTexture();
    }

    private void UnZipData(string fileName)
    {
        if (File.Exists(Application.persistentDataPath + "/" + fileName))
        {
            UnZipUtil.ExtractTGZ(Application.persistentDataPath + "/" + fileName, Application.persistentDataPath);
            Debug.Log("UnZipping Done");
        }
        else
        {
            Debug.LogError(fileName + " not found!");
        }
    }
}