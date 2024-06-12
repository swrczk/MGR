using Analysis;
using Manager;
using Utils.MVC;

namespace UI.ImageProcessingScene
{
    public class ImageProcessingController : BaseController<ImageProcessingView, ImageProcessingContext>
    {
        private ScreenModel _screenModelData;

        public override void Setup()
        {
            base.Setup();
            _screenModelData = Context.ResultsManager.GetFirstSelectedScreen();
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