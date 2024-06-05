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

            var preview = _screenForAnalysisData.ProcessedImages[ImageProcessingType.SegmentedTexture];
            View.ShowProcessedImage(preview);
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
    }
}