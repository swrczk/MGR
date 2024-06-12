using Analysis;
using Analysis.AccessibilityProcessingStage;
using Analysis.PropertiesProcessingStage;
using Analysis.PropertiesProcessingStage.Model;
using Analysis.ReportGenerationStage;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Manager
{
    public class ImageProcessingManager : BaseManager
    {
        [SerializeField]
        private ImageProcessingParametersSo imageProcessingParametersSo;

        [SerializeField]
        private AnalysisParametersSO analysisParametersSo;

        private ScreenForAnalysisSo _screenForAnalysisData;
        private PreprocessingStage _preprocessingStage;
        private PropertiesProcessingStage _propertiesProcessingStage;
        private AccessibilityProcessingStage _accessibilityProcessingStage;
        private ReportGenerationStage _reportGenerationStage;


        public async UniTask AnaliseRefactored(ScreenForAnalysisSo screen)
        {
            InitStages();
            await UniTask.WaitForFixedUpdate();

            _screenForAnalysisData = screen;
            var texture = _screenForAnalysisData.texture;

            var preprocessedModel = await _preprocessingStage.Run(texture);
            SavePreprocessingStageResults(preprocessedModel);

            var propertiesModel = _propertiesProcessingStage.Run(preprocessedModel, texture);
            SavePropertiesProcessingResults(propertiesModel);

            var accessibilityModel = _accessibilityProcessingStage.Run(propertiesModel, preprocessedModel.DefaultMat,
                preprocessedModel.ColorSegmentedMat);
            
            var analysisModel = new AnalysisModel()
            {
                ScreenData = _screenForAnalysisData,
                AnalysisParameters = analysisParametersSo,
                ImageProcessingParameters = imageProcessingParametersSo,
                PreprocessedModel = preprocessedModel,
                PropertiesModel = propertiesModel,
                AccessibilityModel = accessibilityModel
            };
            DataManager.AnalysisModel = analysisModel;
            _reportGenerationStage.Run(analysisModel);
            
        }

        private void InitStages()
        {
            _preprocessingStage = new PreprocessingStage(imageProcessingParametersSo);
            _propertiesProcessingStage = new PropertiesProcessingStage(imageProcessingParametersSo);
            _accessibilityProcessingStage = new AccessibilityProcessingStage(analysisParametersSo);
            _reportGenerationStage = new ReportGenerationStage();
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
            _screenForAnalysisData.SaveProcessedImage(
                preprocessedModel.SegmentedTextureModel.VisualizationMat.ToTexture(),
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