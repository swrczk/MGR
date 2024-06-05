using System;
using System.Collections.Generic;
using System.IO;
using OpenCvSharp;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using Rect = OpenCvSharp.Rect;

namespace Analysis
{
    public enum ImageProcessingType
    {
        ColorSegmentedTexture,
        ContoursTexture,
        OcrHighlightedResults,
        SegmentedTexture,
        ContrastValuesInterpretation,
        Test
    }

    public class ProjectData
    {
        public List<ScreenData> ScreenData { get; set; }
        public float OverallResult { get; set; }
    }

    public class ScreenData
    {
        public string Name { get; set; }
        public ScreenForAnalysisSo ScreenForAnalysisSo { get; set; }
        public PreprocessedTexture PreprocessedTexture { get; set; }
        public TextureProperties TextureProperties { get; set; }
        public AnalysisData AnalysisData { get; set; }
        public float OverallResult { get; set; }
    }

    public class PreprocessedTexture
    {
        public Dictionary<ImageProcessingType, Texture2D> ProcessedImages { get; } = new();
        public TextDocument TextDocument { get; set; }
        public OcrResult OcrResult { get; set; }

        private List<string> FilePaths { get; } = new List<string>(); // co z tym? czy jest wgl potrzebne?
        public List<Rect> SegmentedByColorRects { get; set; }
    }

    //przeneisione do tesseraxra
    // public class OcrResult
    // {
    //     public List<string> Text { get; set; }
    //     public List<Box> Boxes { get; set; }
    //     public Segment Segment { get; set; }//przenieść do result lub usunac
    //     public Texture2D HighlightedDetectedWords { get; set; }
    // }

    public class TextureProperties
    {
        public List<Color> DominantColors { get; set; }
        public Dictionary<ImageProcessingType, Texture2D> ProcessedImages { get; } = new();
        public TextDocument TextDocument { get; set; }

        private List<string> FilePaths { get; } = new List<string>(); // co z tym? czy jest wgl potrzebne?
        public SegmentedTexture SegmentedTexture { get; set; }
    }

    public class SegmentedTexture
    {

        public Rect ImageRect { get; set; }
        public List<Segment> Segments { get; set; }
        public Mat VisualizationMat { get; set; }
        
        public SegmentedTexture(Rect imageRect, List<Segment> segments, Mat visualizationMat)
        {
            ImageRect = imageRect;
            Segments = segments;
            VisualizationMat = visualizationMat;
            RecalculateVolumePercentage();
        }

        public void RecalculateVolumePercentage()
        {
            float imageArea = ImageRect.Width * ImageRect.Height;

            foreach (var segment in Segments)
            {
                float rectArea = segment.Rect.Width * segment.Rect.Height;
                float volumePercentage = (rectArea / imageArea) * 100;
                segment.VolumePercentage = volumePercentage;
            }
        }
    }

    public class Segment
    {
        public Rect Rect { get; set; }
        public float VolumePercentage { get; set; }
    }

    public enum ContrastResultType
    {
        Low,
        High,
        Normal
    }

    public class ContrastAnalysisResult
    {
        public Segment Segment { get; private set; }
        public float Contrast { get; private set; }
        public ContrastResultType ContrastResultType { get; private set; }

        public ContrastAnalysisResult(Segment segment, ContrastResultType low, float overallContrast)
        {
            Segment = segment;
            ContrastResultType = low;
            Contrast = overallContrast;
        }
    }

    public class TextAnalysisResult
    {
        public TextDocument TextDocument { get; set; }
        //zastanowić się nad tym
    }

    public class AnalysisData
    {
        public List<Rect> SegmentedByColorRects { get; set; }
        public Dictionary<ImageProcessingType, Texture2D> ProcessedImages { get; } = new();
        public TextAnalysisResult TextAnalysisResult { get; set; }
        public List<ContrastAnalysisResult> ContrastResults { get; } = new();
    }

    [CreateAssetMenu(fileName = "SFA", menuName = "ScreenForAnalysisSO")]
    public class ScreenForAnalysisSo : ScriptableObject
    {
        public string fileName;
        public string saveFilesPath;
        public Texture2D texture;
        public bool isSelected;

        //-----------------Analysis Data-----------------
        public List<Color> dominantColors;
        public Dictionary<ImageProcessingType, Texture2D> ProcessedImages = new();
        public TextDocument textDocument;

        private List<string> _filePaths = new List<string>();
        public List<Segment> SegmentedByColorRects { get; set; }

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
            var path = SaveAsAsset(texture2D, folder, type.ToString());
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
        }

        public string SaveAsAsset(Object element, string folder, string suffix)
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