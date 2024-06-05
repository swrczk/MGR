using Analysis;
using Analysis.PropertiesProcessingStage;
using Cysharp.Threading.Tasks;
using OpenCvSharp;
using UI.ImageProcessingScene;
using UnityEngine;
using Utils.MVC;

namespace UI
{
    public class ImageProcessingController : BaseController<ImageProcessingView, ImageProcessingContext>
    {
        private ScreenForAnalysisSo _screenForAnalysisData;
        private PreprocessingStage _preprocessingStage = new();
        private PropertiesProcessingStage _propertiesProcessingStage = new();

        public override void Setup()
        {
            base.Setup();
            _screenForAnalysisData = Context.ResultsManager.GetFirstSelectedScreen();
            View.Setup(_screenForAnalysisData);
            AnaliseRefactored().Forget();
            // Analise();
        }

        protected override void CreateDispatcher()
        {
            base.CreateDispatcher();
            View.BackButtonClicked += Back;
        }

        private void Back()
        {
            Context.ChangeScene("Scenes/ConfigScene");
        }

        private void Analise()
        {
            Context.ImageProcessingManager.SetupInput(_screenForAnalysisData);
            Context.ImageProcessingManager.AnaliseColors();
            Context.ImageProcessingManager.AnaliseText();
            Context.ImageProcessingManager.AnaliseContrast();


            var preview = _screenForAnalysisData.ProcessedImages[ImageProcessingType.SegmentedTexture];
            View.ShowProcessedImage(preview);
        }

        private async UniTask AnaliseRefactored()
        {
            var texture = _screenForAnalysisData.texture;
            
            var preprocessedModel = await _preprocessingStage.Run(texture);
            SavePreprocessingStageResults(preprocessedModel);
            
            var propertiesModel = _propertiesProcessingStage.Run(preprocessedModel, texture);
            SavePropertiesProcessingResults(propertiesModel);

            var preview = _screenForAnalysisData.ProcessedImages[ImageProcessingType.SegmentedTexture];
            View.ShowProcessedImage(preview);
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

    public static class ExtenstionAnalysis
    {
        public static Texture2D ToTexture(this Mat mat)
        {
            return AnalysisUtils.MatToTexture(mat);
        }
    }
}