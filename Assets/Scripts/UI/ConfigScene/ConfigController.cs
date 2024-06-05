using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Utils.Common;
using Utils.MVC;

namespace UI.ConfigScene
{
    public class ConfigController : BaseController<ConfigView, ConfigContext>
    {
        protected override void CreateDispatcher()
        {
            base.CreateDispatcher();
            View.UploadButtonClicked += SaveFile;
            View.RemoveElementClicked += RemoveElement;
            View.AnaliseButtonClicked += Analise;
        }

        public override void Setup()
        {
            base.Setup();
            var screenDataArray = Context.ResultsManager.ScreensDictionary.Values.ToArray();
            View.DisplayFiles(screenDataArray);
        }

        private void Analise()
        {
            RunAnalysis().Forget();
        }

        private async UniTaskVoid RunAnalysis()
        {
            var screenData = Context.ResultsManager.GetFirstSelectedScreen();
            await Context.ImageProcessingManager.AnaliseRefactored(screenData);
            Context.ChangeScene("Scenes/ImageProcessingScene");
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
    }
}