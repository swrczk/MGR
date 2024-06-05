using System.Collections.Generic;
using Analysis;
using Cysharp.Threading.Tasks;
using UnityEngine;

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

        [SerializeField]
        private TextureContrastProcessor textureContrastProcessor;
        
        [SerializeField]
        private TextureColorSegmentationProcessor textureColorSegmentationProcessor; 

        private ScreenForAnalysisSo _screenForAnalysisData;

        public void SetupInput(ScreenForAnalysisSo screenForAnalysisData)
        {
            _screenForAnalysisData = screenForAnalysisData;
        }

        public void AnaliseText()
        {
            DetectText().Forget();
        }

        public void AnaliseContrast()
        {
            var words = _screenForAnalysisData.textDocument?.GetWords() ?? new List<Word>();
            var excludeWords = words.Count>0;
            var original = _screenForAnalysisData.texture;
            var segmentedTextureByColors = textureColorSegmentationProcessor.ProcessTexture(original, excludeWords, words);
            _screenForAnalysisData.SegmentedByColorRects = segmentedTextureByColors.finalSegments;
            var result = textureContrastProcessor.CheckContrastInBlocks(segmentedTextureByColors.finalSegments, original); 
            _screenForAnalysisData.SaveProcessedImage(result.outputTexture, "ProcessedTextures", ImageProcessingType.SegmentedTexture);
            _screenForAnalysisData.SaveProcessedImage(segmentedTextureByColors.outputTexture, "ProcessedTextures",
                ImageProcessingType.ContrastValuesInterpretation); 
        }


        public Texture2D AnaliseColors()
        {
            var dominantColors = GetDominantColors();
            var segmentedTexture = GetSegmentedTexture(dominantColors.Count);
            var processedTexture = DrawContours(segmentedTexture);

            _screenForAnalysisData.SaveDominantColors(dominantColors);
            _screenForAnalysisData.SaveProcessedImage(segmentedTexture, "ProcessedTextures",
                ImageProcessingType.ColorSegmentedTexture);
            var preview = _screenForAnalysisData.SaveProcessedImage(processedTexture, "ProcessedTextures",
                ImageProcessingType.ContoursTexture);
            return preview;
        }

        private async UniTask DetectText()
        {
            var colorSegmentationTexture =
                _screenForAnalysisData.ProcessedImages[ImageProcessingType.ColorSegmentedTexture]; 
            var ocrResult = await textDetectionProcessor.Process(colorSegmentationTexture);
            _screenForAnalysisData.SaveProcessedImage(ocrResult.HighlightedTexture, "ProcessedTextures",
                ImageProcessingType.OcrHighlightedResults);

            var textDocument = textDetectionProcessor.GetTextDocument(ocrResult);
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
            return colorDetectionProcessor.DrawDetectedContours(texture);
        }
    }
}