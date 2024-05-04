using System;
using System.Collections.Generic;
using System.IO;
using Analysis;
using UnityEditor;
using UnityEngine;
using Utils;

public class ResultsManager : BaseManager
{
    private const string SaveFilesPath = "Assets/Resources/Screens";
    public Dictionary<string, ScreenDTO> ScreensDictionary { get; } = new Dictionary<string, ScreenDTO>();

    public override void Initialize()
    {
        base.Initialize();
        LoadFiles();
    }


    public ScreenDTO CreateScreenDto(Texture2D texture)
    {
        long time = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
        ScreenDTO screenDto = ScriptableObject.CreateInstance<ScreenDTO>();
        screenDto.texture = texture;
        screenDto.texturePath = SaveFilesPath;
        screenDto.fileName = $"{time}";
        screenDto.SaveTextureAsAsset(texture, "original");
        ScreensDictionary.Add(screenDto.name, screenDto);

        AssetDatabase.CreateAsset(screenDto, SaveFilesPath + $"/{time}_DTO.asset");
        RefreshAssets();
        return screenDto;
    }

    public void DeleteScreenDto(string key)
    {
        if (ScreensDictionary.TryGetValue(key, out ScreenDTO screenDto))
        {
            screenDto.Delete();
            ScreensDictionary.Remove(key);
        }
    }

    private void LoadFiles()
    {
        var screenDataArray = Resources.LoadAll<ScreenDTO>("");
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