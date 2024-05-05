using System;
using System.Collections.Generic;
using System.IO;
using Analysis;
using UnityEditor;
using UnityEngine;
using Utils;
using Object = UnityEngine.Object;

public class ResultsManager : BaseManager
{
    private const string SaveFilesPath = "Assets/Resources/Screens";
    public Dictionary<string, ScreenSo> ScreensDictionary { get; } = new Dictionary<string, ScreenSo>();

    public override void Initialize()
    {
        base.Initialize();
        LoadFiles();
    }


    public ScreenSo CreateScreenDto(Texture2D texture)
    {
        long time = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
        ScreenSo screenSo = ScriptableObject.CreateInstance<ScreenSo>();
        screenSo.texture = texture;
        screenSo.texturePath = SaveFilesPath;
        screenSo.thisObjectPath = $"{SaveFilesPath}/{time}_DTO.asset";
        screenSo.fileName = $"{time}";
        screenSo.SaveAsAsset(screenSo,"DTO");
        ScreensDictionary.Add(screenSo.name, screenSo);

        AssetDatabase.CreateAsset(screenSo, screenSo.thisObjectPath);
        RefreshAssets();
        return screenSo;
    }

    public void DeleteScreenDto(string key)
    {
        if (ScreensDictionary.TryGetValue(key, out ScreenSo screenDto))
        {
            screenDto.Delete();
            ScreensDictionary.Remove(key);
        }
    }

    private void LoadFiles()
    {
        var screenDataArray = Resources.LoadAll<ScreenSo>("");
        foreach (var dataSo in screenDataArray)
        {
            ScreensDictionary.Add(dataSo.name, dataSo);
        }
    }

    public static void RefreshAssets()
    {
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void SaveTextureAsPNG(Texture2D texture2D, string filePath)
    {
        byte[] pngBytes = texture2D.EncodeToPNG();
        File.WriteAllBytes(filePath, pngBytes);
    }
}