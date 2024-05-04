using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Analysis
{
    [CreateAssetMenu(fileName = "Screen", menuName = "ScreenSO")]
    public class ScreenDTO : ScriptableObject
    {
        public string fileName;
        public string texturePath;
        public string thisObjectPath;
        public Texture2D texture;
        public List<Color> dominantColors;
        public Texture2D colorSegmentation;

        public void Delete()
        {
            AssetDatabase.DeleteAsset(texturePath);
            AssetDatabase.DeleteAsset(thisObjectPath);
            RefreshAssets();
        }

        public void SaveDominantColors(List<Color> colors)
        {
            dominantColors = colors;
        }

        public void SaveColorSegmentation(Texture2D texture2D)
        {
            colorSegmentation = texture2D;
            var path = texturePath.Split(".asset")[0];
            SaveTextureAsAsset(texture2D, "colorSegmentation");
            SaveTextureAsPNG(texture2D, path + "_colorSegmentation.png");
            RefreshAssets();
        }

        public void SaveTextureAsAsset(Texture2D texture2D, string suffix)
        {
            AssetDatabase.CreateAsset(texture2D, $"{texturePath}/{fileName}_{suffix}.asset");
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
    }
}