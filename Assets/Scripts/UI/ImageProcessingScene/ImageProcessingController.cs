using Analysis;
using UI.ImageProcessingScene;
using Utils.Common;
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
            var preview = Context.ImageProcessingManager.AnaliseColors();
            View.ShowProcessedImage(preview);
            Context.ImageProcessingManager.AnaliseText();
        }
    }
}