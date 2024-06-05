using Manager;
using Utils.MVC;

namespace UI.ConfigScene
{
    public class ConfigContext : BaseContext
    {
        public ResultsManager ResultsManager { get; private set; }
        public UploadFileManager UploadFileManager { get; private set; }
        public ImageProcessingManager ImageProcessingManager { get; private set; }

        public override void ResolveContext()
        {
            base.ResolveContext();
            UploadFileManager = Managers.Instance.GetManager<UploadFileManager>();
            ResultsManager = Managers.Instance.GetManager<ResultsManager>();
            ImageProcessingManager = Managers.Instance.GetManager<ImageProcessingManager>();
        }
    }
}