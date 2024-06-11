using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Analysis;
using UnityEditor;
using UnityEngine;

namespace Manager
{
    public class ResultsManager : BaseManager
    {
        private const string SaveFilesPath = "Assets/Resources/Screens";

        public Dictionary<string, ScreenForAnalysisSo> ScreensDictionary { get; } =
            new Dictionary<string, ScreenForAnalysisSo>();

        public override void Initialize()
        {
            base.Initialize();
            LoadFiles();
        }


        public ScreenForAnalysisSo CreateScreenDto(Texture2D texture, string screenName)
        {
            long time = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
            ScreenForAnalysisSo screenForAnalysisSo = ScriptableObject.CreateInstance<ScreenForAnalysisSo>();
            screenForAnalysisSo.saveFilesPath = SaveFilesPath;
            screenForAnalysisSo.fileName = $"{time}";
            screenForAnalysisSo.SaveMainTexture(texture);
            screenForAnalysisSo.SaveAsAsset(screenForAnalysisSo, "DTO");
            screenForAnalysisSo.screenName = screenName.Replace(" ", "_");
            ScreensDictionary.Add(screenForAnalysisSo.name, screenForAnalysisSo);
            //
            // AssetDatabase.CreateAsset(screenSo, screenSo.saveFilesPath);
            RefreshAssets();
            return screenForAnalysisSo;
        }

        public void DeleteScreenDto(string key)
        {
            if (ScreensDictionary.TryGetValue(key, out ScreenForAnalysisSo screenDto))
            {
                screenDto.Delete();
                ScreensDictionary.Remove(key);
            }
        }

        private void LoadFiles()
        {
            var screenDataArray = Resources.LoadAll<ScreenForAnalysisSo>("");
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

        public ScreenForAnalysisSo GetFirstSelectedScreen()
        {
            foreach (var screen in ScreensDictionary.Values)
            {
                if (screen.isSelected)
                {
                    return screen;
                }
            }

            return ScreensDictionary.Values.First();
        }

        private void SaveTextureAsPNG(Texture2D texture2D, string filePath)
        {
            byte[] pngBytes = texture2D.EncodeToPNG();
            File.WriteAllBytes(filePath, pngBytes);
        }
    }
}