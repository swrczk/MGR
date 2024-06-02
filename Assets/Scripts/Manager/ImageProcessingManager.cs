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

        [SerializeField]
        private TextureContrastProcessor textureContrastProcessor;

        [FormerlySerializedAs("textureEntropyProcessor")]
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
            _screenForAnalysisData.SegmentedByColorRects = segmentedTextureByColors.finalRects;
            var result = textureContrastProcessor.CheckContrastInBlocks(segmentedTextureByColors.finalRects, original); 
            _screenForAnalysisData.SaveProcessedImage(result, "ProcessedTextures", ImageProcessingType.ContrastBlocks);
            _screenForAnalysisData.SaveProcessedImage(segmentedTextureByColors.outputTexture, "ProcessedTextures",
                ImageProcessingType.ColorBlocksPreprocessed); 
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

        private async UniTask DetectText()
        {
            var colorSegmentationTexture =
                _screenForAnalysisData.ProcessedImages[ImageProcessingType.ColorSegmentation]; 
            var highlightedTextTexture = await textDetectionProcessor.Process(colorSegmentationTexture);
            _screenForAnalysisData.SaveProcessedImage(highlightedTextTexture, "ProcessedTextures",
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