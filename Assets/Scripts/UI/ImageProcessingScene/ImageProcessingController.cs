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


            var preview = _screenForAnalysisData.ProcessedImages[ImageProcessingType.ContrastBlocks];
            View.ShowProcessedImage(preview);
        }

        private async UniTask AnaliseRefactored()
        {
            var preprocessedModel = await _preprocessingStage.Run(_screenForAnalysisData.texture);
            var propertiesModel = _propertiesProcessingStage.Run(preprocessedModel);

            _screenForAnalysisData.SaveDominantColors(preprocessedModel.DominantColors);
            _screenForAnalysisData.SaveProcessedImage(preprocessedModel.ContoursTexture, "ProcessedTextures",
                ImageProcessingType.ColorContours);
            _screenForAnalysisData.SaveProcessedImage(preprocessedModel.ColorSegmentedMat.ToTexture(),
                "ProcessedTextures",
                ImageProcessingType.ColorSegmentation);
            _screenForAnalysisData.SaveProcessedImage(preprocessedModel.OcrResult.HighlightedTexture,
                "ProcessedTextures",
                ImageProcessingType.OcrHighlightedTexture);
            _screenForAnalysisData.SaveProcessedImage(preprocessedModel.SegmentedTexture.VisualizationMat.ToTexture(),
                "ProcessedTextures", ImageProcessingType.ContrastBlocks);

            _screenForAnalysisData.SaveProcessedImage(propertiesModel.ContrastVisualizationMat.ToTexture(),
                "ProcessedTextures",
                ImageProcessingType.ColorBlocksPreprocessed);

            var preview = _screenForAnalysisData.ProcessedImages[ImageProcessingType.ContrastBlocks];
            View.ShowProcessedImage(preview);
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