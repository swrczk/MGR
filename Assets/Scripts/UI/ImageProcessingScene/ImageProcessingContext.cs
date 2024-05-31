using Manager;
using Utils;
using Utils.Common;
using Utils.MVC;

namespace UI.ImageProcessingScene
{
    public class ImageProcessingContext : BaseContext
    {
        public ImageProcessingManager ImageProcessingManager { get; private set; }
        public ResultsManager ResultsManager { get; private set; }

        public override void ResolveContext()
        {
            base.ResolveContext();
            ImageProcessingManager = Managers.Instance.GetManager<ImageProcessingManager>();
            ResultsManager = Managers.Instance.GetManager<ResultsManager>();
        }
    }
}