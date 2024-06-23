using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Serialization;

namespace Analysis
{
    [Serializable]
    public class ScreenModel
    {
        public string uploadTimeStamp;
        public string name;
        public string saveFilesPath;
        public Texture2D texture;
        public bool isSelected;
        public List<Color> dominantColors;
        private readonly List<string> _filePaths = new();
        
        public string pathToReport;

        public void Delete()
        {
            foreach (var fileName in _filePaths)
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }

            _filePaths.Clear();
        }

        public void SaveDominantColors(List<Color> colors)
        {
            dominantColors = colors;
        }

        public string SaveAsPNG(Texture2D texture2D, string folder, string suffix)
        {
            var time = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
            var path = Path.Combine(saveFilesPath, folder, $"{uploadTimeStamp}_{suffix}_{time}.png");
            var pngBytes = texture2D.EncodeToPNG();
            Directory.CreateDirectory(Path.GetDirectoryName(path)); // Ensure directory exists
            File.WriteAllBytes(path, pngBytes);
            _filePaths.Add(path);
            return path;
        }

        public void SaveMainTexture(Texture2D texture2D)
        {
            var path = SaveAsPNG(texture2D, "texture", "main");
            texture2D.name = Path.GetFileNameWithoutExtension(path);
            texture = texture2D;
        }
    }
}