using Analysis;
using UI.Common;
using UnityEditor.Timeline;

namespace UI
{
    public class ImageProcessingController : BaseController<ImageProcessingView, ImageProcessingContext>
    {
        public void Start()
        {
            Context.ImageProcessingManager.SetupInput(View.ScreenForAnalysisData);
            var preview = Context.ImageProcessingManager.AnaliseColors();
            View.ShowProcessedImage(preview);
            Context.ImageProcessingManager.AnaliseText();
        }
    }
}