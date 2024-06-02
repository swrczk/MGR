using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using Rect = OpenCvSharp.Rect;

namespace Analysis
{
    public enum ImageProcessingType
    {
        ColorSegmentation,
        ColorContours,
        TextContours,
        ContrastBlocks,
        ColorBlocksPreprocessed,
        Test
    }
    
    [CreateAssetMenu(fileName = "SFA", menuName = "ScreenForAnalysisSO")]
    public class ScreenForAnalysisSo : ScriptableObject
    {
        public string fileName;
        public string saveFilesPath;
        public Texture2D texture;
        public List<Color> dominantColors;
        public Dictionary<ImageProcessingType, Texture2D> ProcessedImages = new();
        public bool isSelected;
        public TextDocument textDocument;

        private List<string> _filePaths = new List<string>();
        public List<Rect> SegmentedByColorRects { get; set; }

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

        public Texture2D SaveProcessedImage(Texture2D texture2D, ImageProcessingType type)
        {
            var path = SaveAsAsset(texture2D, type.ToString());
            texture2D.name = path.Split("/")[^1].Split(".asset")[0];
            SaveAsPNG(texture2D, type.ToString());
            ProcessedImages[type] = texture2D;
            return texture2D;
        }

        public Texture2D SaveProcessedImage(Texture2D texture2D, string folder, ImageProcessingType type)
        {
            var path = SaveAsAsset(texture2D,folder, type.ToString());
            texture2D.name = path.Split("/")[^1].Split(".asset")[0];
            SaveAsPNG(texture2D, type.ToString());
            ProcessedImages[type] = texture2D;
            return texture2D;
        }
        public string SaveAsAsset(Object element, string suffix)
        {
            var path = $"{saveFilesPath}/{fileName}_{suffix}.asset";
            AssetDatabase.CreateAsset(element, path);
            _filePaths.Add(path);
            return path;
        }        public string SaveAsAsset(Object element,string folder, string suffix)
        {
            var path = $"{saveFilesPath}/{folder}/{fileName}_{suffix}.asset";
            AssetDatabase.CreateAsset(element, path);
            _filePaths.Add(path);
            return path;
        }

        private static void RefreshAssets()
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public string SaveAsPNG(Texture2D texture2D, string suffix)
        {
            long time = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
            var path = $"{saveFilesPath}/SavedAsPng/{fileName}_{suffix}_{time}.png";
            byte[] pngBytes = texture2D.EncodeToPNG();
            File.WriteAllBytes(path, pngBytes);
            _filePaths.Add(path);
            return path;
        }


        public void SaveMainTexture(Texture2D texture2D)
        {
            var path = SaveAsAsset(texture2D, "texture");
            texture2D.name = path.Split("/")[^1].Split(".asset")[0];
            texture = texture2D;
        }

        public void SaveTextDocument(TextDocument obj)
        {
            textDocument = obj;
        }
    }
}