using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Analysis;
using UnityEngine;

namespace Manager
{
    public class ResultsManager : BaseManager
    {
        private string SaveFilesPath => Path.Combine(Application.persistentDataPath, "Screens");

        public Dictionary<string, ScreenModel> ScreensDictionary { get; } = new();

        public override void Initialize()
        {
            base.Initialize();
            CreateSaveDirectory();
            LoadFiles();
        }

        public ScreenModel CreateScreenDto(Texture2D texture, string screenName)
        {
            var time = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
            var screenModel = new ScreenModel
            {
                saveFilesPath = SaveFilesPath,
                uploadTimeStamp = $"{time}",
                name = screenName.Replace(" ", "_"),
            };
            screenModel.SaveMainTexture(texture);
            SaveAsAsset(screenModel, "DTO");

            var uniqueKey = screenModel.uploadTimeStamp;
            while (ScreensDictionary.ContainsKey(uniqueKey))
            {
                uniqueKey = $"{screenModel.uploadTimeStamp}_{Guid.NewGuid()}";
            }

            ScreensDictionary.Add(uniqueKey, screenModel);
            return screenModel;
        }

        public void DeleteScreenDto(string key)
        {
            if (ScreensDictionary.TryGetValue(key, out var screenModel))
            {
                screenModel.Delete();
                DeleteAssetFiles(screenModel);
                ScreensDictionary.Remove(key);
            }
        }
        public void ToggleScreenSelection(string key, bool isSelected)
        {
            if (ScreensDictionary.TryGetValue(key, out var screenModel))
            {
                screenModel.isSelected = isSelected;
                SaveAsAsset(screenModel, "DTO"); // Save changes to disk
            }
        }

        private void LoadFiles()
        {
            var screenDataArray = LoadAllScreens();
            foreach (var data in screenDataArray)
            {
                var uniqueKey = data.uploadTimeStamp;
                while (ScreensDictionary.ContainsKey(uniqueKey))
                {
                    uniqueKey = $"{data.uploadTimeStamp}_{Guid.NewGuid()}";
                }

                ScreensDictionary.Add(uniqueKey, data);
            }
        }

        public ScreenModel GetFirstSelectedScreen()
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
        
        public List<ScreenModel> GetSelectedScreens()
        {
            var selectedScreens = new List<ScreenModel>();
            foreach (var screen in ScreensDictionary.Values)
            {
                if (screen.isSelected)
                {
                    selectedScreens.Add(screen);
                }
            }

            return selectedScreens;
        }

        private void SaveAsAsset(ScreenModel screenModel, string folder)
        {
            var path = Path.Combine(SaveFilesPath, folder, $"{screenModel.uploadTimeStamp}.json");
            var json = JsonUtility.ToJson(screenModel);
            Directory.CreateDirectory(Path.GetDirectoryName(path)); // Ensure directory exists
            File.WriteAllText(path, json);

            if (screenModel.texture != null)
            {
                SaveTextureAsPNG(screenModel.texture, Path.Combine(SaveFilesPath, folder, $"{screenModel.uploadTimeStamp}.png"));
            }
        }

        private void SaveTextureAsPNG(Texture2D texture2D, string filePath)
        {
            var pngBytes = texture2D.EncodeToPNG();
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)); // Ensure directory exists
            File.WriteAllBytes(filePath, pngBytes);
        }

        private Texture2D LoadTextureFromPNG(string filePath)
        {
            if (File.Exists(filePath))
            {
                var pngBytes = File.ReadAllBytes(filePath);
                var texture = new Texture2D(2, 2);
                texture.LoadImage(pngBytes);
                return texture;
            }
            return null;
        }

        private ScreenModel[] LoadAllScreens()
        {
            var screensList = new List<ScreenModel>();
            if (Directory.Exists(SaveFilesPath))
            {
                var files = Directory.GetFiles(SaveFilesPath, "*.json", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    var json = File.ReadAllText(file);
                    var screen = new ScreenModel();
                    JsonUtility.FromJsonOverwrite(json, screen);
                    var texturePath = file.Replace(".json", ".png");
                    screen.texture = LoadTextureFromPNG(texturePath);
                    screensList.Add(screen);
                }
            }
            return screensList.ToArray();
        }

        private void CreateSaveDirectory()
        {
            if (!Directory.Exists(SaveFilesPath))
            {
                Directory.CreateDirectory(SaveFilesPath);
            }
        }

        private void DeleteAssetFiles(ScreenModel screenModel)
        {
            var jsonPath = Path.Combine(screenModel.saveFilesPath, "DTO", $"{screenModel.uploadTimeStamp}.json");
            if (File.Exists(jsonPath))
            {
                File.Delete(jsonPath);
            }

            var pngPath = Path.Combine(screenModel.saveFilesPath, "DTO", $"{screenModel.uploadTimeStamp}.png");
            if (File.Exists(pngPath))
            {
                File.Delete(pngPath);
            }
        }
    }
}
