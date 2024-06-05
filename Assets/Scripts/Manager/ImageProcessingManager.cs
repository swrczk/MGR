using Analysis;
using Analysis.PropertiesProcessingStage;
using Cysharp.Threading.Tasks;

namespace Manager
{
    public class ImageProcessingManager : BaseManager
    {
        private ScreenForAnalysisSo _screenForAnalysisData;
        private PreprocessingStage _preprocessingStage = new();
        private PropertiesProcessingStage _propertiesProcessingStage = new();


        public async UniTask AnaliseRefactored(ScreenForAnalysisSo screen)
        {
            await UniTask.WaitForFixedUpdate();
            _screenForAnalysisData = screen;
            var texture = _screenForAnalysisData.texture;

            var preprocessedModel = await _preprocessingStage.Run(texture);
            SavePreprocessingStageResults(preprocessedModel);

            var propertiesModel = _propertiesProcessingStage.Run(preprocessedModel, texture);
            SavePropertiesProcessingResults(propertiesModel);
        }

        private void SavePreprocessingStageResults(PreprocessedModel preprocessedModel)
        {
            _screenForAnalysisData.SaveDominantColors(preprocessedModel.DominantColors);
            _screenForAnalysisData.SaveProcessedImage(preprocessedModel.ContoursTexture, "ProcessedTextures",
                ImageProcessingType.ContoursTexture);
            _screenForAnalysisData.SaveProcessedImage(preprocessedModel.ColorSegmentedMat.ToTexture(),
                "ProcessedTextures",
                ImageProcessingType.ColorSegmentedTexture);
            _screenForAnalysisData.SaveProcessedImage(preprocessedModel.OcrResult.HighlightedTexture,
                "ProcessedTextures",
                ImageProcessingType.OcrHighlightedResults);
            _screenForAnalysisData.SaveProcessedImage(preprocessedModel.SegmentedTexture.VisualizationMat.ToTexture(),
                "ProcessedTextures", ImageProcessingType.SegmentedTexture);
        }

        private void SavePropertiesProcessingResults(PropertiesModel propertiesModel)
        {
            _screenForAnalysisData.SaveProcessedImage(propertiesModel.ContrastVisualizationMat.ToTexture(),
                "ProcessedTextures",
                ImageProcessingType.ContrastValuesInterpretation);
        }
    }
}