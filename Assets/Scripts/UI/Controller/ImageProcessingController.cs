using Analysis;
using UI.Common;

namespace UI
{
    public class ImageProcessingController : BaseController<ImageProcessingView, ImageProcessingContext>
    {
        public void Start()
        {
            Context.ImageProcessingManager.SetupInput(View.ScreenForAnalysisData);
            var dominantColors = Context.ImageProcessingManager.GetDominantColors();
            var segmentedTexture = Context.ImageProcessingManager.GetSegmentedTexture(dominantColors.Count);
            var processedTexture = Context.ImageProcessingManager.DrawContours(segmentedTexture);

            View.ScreenForAnalysisData.SaveDominantColors(dominantColors);
            View.ScreenForAnalysisData.SaveProcessedImage(segmentedTexture, ImageProcessingType.ColorSegmentation);
            var preview = View.ScreenForAnalysisData.SaveProcessedImage(processedTexture, ImageProcessingType.Contours);
            View.ShowProcessedImage(preview);
        }
    }
}