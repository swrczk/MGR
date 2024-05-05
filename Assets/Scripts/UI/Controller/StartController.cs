using System.Linq;
using Cysharp.Threading.Tasks;
using UI.Common;
using UnityEngine;

namespace UI
{
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
                var texture = (savedFile.Item2 as Texture2D);
                var so = Context.ResultsManager.CreateScreenDto(texture);
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
}