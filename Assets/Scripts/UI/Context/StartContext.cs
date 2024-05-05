using Manager;
using Utils;

namespace UI
{
    public class StartContext : BaseContext
    {
        public ResultsManager ResultsManager { get; private set; }
        public UploadFileManager UploadFileManager { get; private set; }

        public override void ResolveContext()
        {
            base.ResolveContext();
            UploadFileManager = Managers.Instance.GetManager<UploadFileManager>();
            ResultsManager = Managers.Instance.GetManager<ResultsManager>();
        }
    }
}