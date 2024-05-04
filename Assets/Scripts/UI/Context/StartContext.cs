using UI.Common;
using Utils;

public class StartContext : BaseContext
{
    public ShowFilesManager ShowFilesManager { get; private set; }
    public UploadFileManager UploadFileManager { get; private set; }

    public override void ResolveContext()
    {
        ShowFilesManager = Managers.Instance.GetManager<ShowFilesManager>();
        UploadFileManager = Managers.Instance.GetManager<UploadFileManager>();
    }
}