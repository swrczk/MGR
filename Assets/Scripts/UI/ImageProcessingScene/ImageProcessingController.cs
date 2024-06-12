using Analysis;
using Manager;
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
            var model = DataManager.AnalysisModel;
            View.Setup(model);
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