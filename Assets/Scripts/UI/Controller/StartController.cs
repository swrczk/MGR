using System.Linq;
using Cysharp.Threading.Tasks;
using UI.Common;
using UnityEditor.Timeline;

public class StartController : BaseController<StartView, StartContext>
{
    protected override void CreateDispatcher()
    {
        base.CreateDispatcher();
        View.UploadButtonClicked += SaveFile;
        View.RemoveElementClicked += RemoveElement;
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
            var so = Context.ResultsManager.CreateScreenDto(savedFile.Item2); 
            View.DisplayFile(so);
        }
    } 

    private void RemoveElement(string key, ScreenElement element)
    {
        Context.ResultsManager.DeleteScreenDto(key);
        Destroy(element.gameObject);
    }

    public override void Setup()
    {
        base.Setup();
        var screenDataArray = Context.ResultsManager.ScreensDictionary.Values.ToArray();
        View.DisplayFiles(screenDataArray);
    }
}