using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Analysis
{
    [CreateAssetMenu(fileName = "Screen", menuName = "ScreenSO")]
    public class ScreenDataSo : ScriptableObject
    {
        private const string saveFilesPath = "Assets/Resources/Screens";
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

        public void SaveColorSegmentation(Texture2D texture2D)
        {
            colorSegmentation = texture2D;
            var path = texturePath.Split(".asset")[0];
            SaveTextureAsAsset(texture2D, "colorSegmentation");
            SaveTextureAsPNG(texture2D, path + "_colorSegmentation.png");
            RefreshAssets();
        }

        private void SaveTextureAsAsset(Texture2D texture2D, string suffix)
        {
            AssetDatabase.CreateAsset(texture2D, $"{texturePath}/{fileName}_{suffix}.asset");
        }

        private static void RefreshAssets()
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public void SaveDominantColors(List<Color> colors)
        {
            dominantColors = colors;
        }

        private void SaveTextureAsPNG(Texture2D texture2D, string filePath)
        {
            byte[] pngBytes = texture2D.EncodeToPNG();
            File.WriteAllBytes(filePath, pngBytes);
        }

        public static ScreenDataSo CreateScreenDataSo(Texture2D texture)
        {
            long time = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
            ScreenDataSo screenDataSo = ScriptableObject.CreateInstance<ScreenDataSo>();
            screenDataSo.texture = texture;
            screenDataSo.texturePath = saveFilesPath;
            screenDataSo.fileName = $"{time}";
            screenDataSo.SaveTextureAsAsset(texture, "original");

            AssetDatabase.CreateAsset(screenDataSo, saveFilesPath + $"/{time}_DTO.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return screenDataSo;
        }
    }
}