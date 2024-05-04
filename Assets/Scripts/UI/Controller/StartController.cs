using Cysharp.Threading.Tasks;
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
        SaveFileAsync().Forget();
    }

    private async UniTaskVoid SaveFileAsync()
    {
        var savedFile = await Context.UploadFileManager.SaveFile();
        if (savedFile.Item1)
        {
            Context.ShowFilesManager.DisplayFile(savedFile.Item2,View.FilePrefab, View.ParentTransform);
        }
    }

    public override void Setup()
    {
        base.Setup();
        Context.ShowFilesManager.DisplayFiles(View.FilePrefab, View.ParentTransform);
    }
}