using Analysis;
using UI.ImageProcessingScene;
using Utils.MVC;

namespace UI
{
    public class ImageProcessingController : BaseController<ImageProcessingView, ImageProcessingContext>
    {
        private ScreenForAnalysisSo _screenForAnalysisData;

        public override void Setup()
        {
            base.Setup();
            _screenForAnalysisData = Context.ResultsManager.GetFirstSelectedScreen();
            View.Setup(_screenForAnalysisData);
            Analise();
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
    }
}