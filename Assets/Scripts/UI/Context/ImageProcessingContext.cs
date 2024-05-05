using Manager;
using Utils;
using Utils.Common;

namespace UI
{
    public class ImageProcessingContext : BaseContext
    {
        public ImageProcessingManager ImageProcessingManager { get; private set; }

        public override void ResolveContext()
        {
            base.ResolveContext();
            ImageProcessingManager = Managers.Instance.GetManager<ImageProcessingManager>();
        }
    }
}