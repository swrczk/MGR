using UI.Common;

public class StartController : BaseController<StartView, StartContext>
{
    protected override void CreateDispatcher()
    {
        base.CreateDispatcher();
        View.UploadButtonClicked += SaveFile;
    }

    private void SaveFile()
    {
        var isSaved = Context.UploadFileManager.SaveFile();
        if (isSaved)
        {
            Context.ShowFilesManager.DisplayFiles(View.FilePrefab, View.ParentTransform);
        }
    }

    public override void Setup()
    {
        base.Setup();
        Context.ShowFilesManager.DisplayFiles(View.FilePrefab, View.ParentTransform);
    }
}