using System.Collections.Generic;
using Analysis;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Utils;

namespace Manager
{
    public class ImageProcessingManager : BaseManager
    {
        [SerializeField]
        private ColorDetectorProcessor colorDetectorProcessor;

        [SerializeField]
        private int clusterCount = 5;
        
        [SerializeField]
        private TextDetectionProcessor textDetectionProcessor;

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
            // await textDetectionProcessor.CheckSerifs(texture) ; //not working :(
        }

        private List<Color> GetDominantColors()
        {
            return colorDetectorProcessor.DetectDominantColors(_screenForAnalysisData.texture, clusterCount);
        }

        private Texture2D GetSegmentedTexture(int dominantColorsCount)
        {
            return colorDetectorProcessor.SegmentImageUsingKMeans(_screenForAnalysisData.texture, dominantColorsCount);
        }

        private Texture2D DrawContours(Texture2D texture)
        {
            return colorDetectorProcessor.DrawContours(texture);
        }
    }
}