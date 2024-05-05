using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Analysis
{
    [CreateAssetMenu(fileName = "Screen", menuName = "ScreenSO")]
    public class ScreenSo : ScriptableObject
    {
        public string fileName;
        public string saveFilesPath;
        public Texture2D texture;
        public List<Color> dominantColors;
        public Texture2D colorSegmentation;

        private List<string> _filePaths = new List<string>();

        public void Delete()
        {
            foreach (var fileName in _filePaths)
            {
                AssetDatabase.DeleteAsset(fileName);
            }

            RefreshAssets();
        }

        public void SaveDominantColors(List<Color> colors)
        {
            dominantColors = colors;
        }

        public void SaveColorSegmentation(Texture2D texture2D)
        {
            colorSegmentation = texture2D;
            var path = saveFilesPath.Split(".asset")[0];
            SaveAsAsset(texture2D, "colorSegmentation");
            SaveTextureAsPNG(texture2D, path + "_colorSegmentation.png");
            RefreshAssets();
        }

        public string SaveAsAsset(Object element, string suffix)
        {
            var path = $"{saveFilesPath}/{fileName}_{suffix}.asset";
            AssetDatabase.CreateAsset(element, path);
            _filePaths.Add(path);
            return path;
        }

        private static void RefreshAssets()
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void SaveTextureAsPNG(Texture2D texture2D, string filePath)
        {
            byte[] pngBytes = texture2D.EncodeToPNG();
            File.WriteAllBytes(filePath, pngBytes);
        }

        public void SaveTexture(Texture2D texture2D)
        {
            var path = SaveAsAsset(texture2D, "texture");
            texture2D.name = path.Split("/")[^1].Split(".asset")[0];
            texture = texture2D;
        }
    }
}