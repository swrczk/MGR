using System.Collections.Generic;
using Analysis;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

namespace Manager
{
    public class ImageProcessingManager : BaseManager
    {
        [SerializeField]
        private ColorDetectionProcessor colorDetectionProcessor;

        [SerializeField]
        private int clusterCount = 5;
        
        [SerializeField]
        private TextDetectionProcessor textDetectionProcessor;
        
        [SerializeField]
        private TextPropertiesProcessor textPropertiesProcessor;

        private ScreenForAnalysisSo _screenForAnalysisData;

        public void SetupInput(ScreenForAnalysisSo screenForAnalysisData)
        {
            _screenForAnalysisData = screenForAnalysisData;
        }


        public Texture2D AnaliseColors()
        {
            var dominantColors = GetDominantColors();
            var segmentedTexture = GetSegmentedTexture(dominantColors.Count);
            var processedTexture = DrawContours(segmentedTexture);

            _screenForAnalysisData.SaveDominantColors(dominantColors);
            _screenForAnalysisData.SaveProcessedImage(segmentedTexture, "ProcessedTextures",
                ImageProcessingType.ColorSegmentation);
            var preview = _screenForAnalysisData.SaveProcessedImage(processedTexture, "ProcessedTextures",
                ImageProcessingType.ColorContours);
            return preview;
        }

        public void AnaliseText()
        {
            DetectText().Forget();
        }

        private async UniTask DetectText()
        {
            var texture = _screenForAnalysisData.ProcessedImages[ImageProcessingType.ColorSegmentation];
            var textContours2 = await textDetectionProcessor.Process(texture);
            _screenForAnalysisData.SaveProcessedImage(textContours2, "ProcessedTextures",
                ImageProcessingType.TextContours);
            var textDocument = textDetectionProcessor.GetTextDocument();
            _screenForAnalysisData.SaveTextDocument(textDocument);
            textPropertiesProcessor.DetectTextColor(_screenForAnalysisData);
        }

        private List<Color> GetDominantColors()
        {
            return colorDetectionProcessor.DetectDominantColors(_screenForAnalysisData.texture, clusterCount);
        }

        private Texture2D GetSegmentedTexture(int dominantColorsCount)
        {
            return colorDetectionProcessor.SegmentImageUsingKMeans(_screenForAnalysisData.texture, dominantColorsCount);
        }

        private Texture2D DrawContours(Texture2D texture)
        {
            return colorDetectionProcessor.DrawContours(texture);
        }
    }
}