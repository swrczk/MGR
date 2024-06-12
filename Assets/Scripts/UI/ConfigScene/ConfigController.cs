using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
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
            View.ToggleElementClicked += ToggleElement;
            View.SettingsButtonClicked += ShowSettings;
            Context.ImageProcessingManager.OnStageCompleted += HandleStageCompleted;
            View.AnalysisSettingsElement.SaveButtonClicked += SaveNewAnalysisSettings;
        }

        public override void Setup()
        {
            base.Setup();
            var screenDataArray = Context.ResultsManager.ScreensDictionary.Values.ToArray();
            View.DisplayFiles(screenDataArray);
            SetupSettingsView();
        }

        private void SetupSettingsView()
        {
            var analysisParameters = Context.ImageProcessingManager.AnalysisParameters;
            View.AnalysisSettingsElement.ApplyAnalysisParameters(analysisParameters);
        }

        private void ShowSettings()
        {
            View.AnalysisSettingsElement.gameObject.SetActive(true);
        }

        private void SaveNewAnalysisSettings()
        {
            var featureSettings = View.AnalysisSettingsElement.FeatureSettings;
            Context.AnalysisSettingsManager.UpdateAndSaveParameters(featureSettings);
            View.AnalysisSettingsElement.gameObject.SetActive(false);
        }

        private void Analise()
        {
            RunAnalysis().Forget();
        }

        private async UniTaskVoid RunAnalysis()
        {
            var model = Context.ResultsManager.GetSelectedScreens();
            await Context.ImageProcessingManager.AnaliseScreens(model);
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
                var screenName = savedFile.Item3;
                var so = Context.ResultsManager.CreateScreenDto(texture, screenName);
                View.DisplayFile(so);
            }
        }

        private void RemoveElement(string key, ScreenElement element)
        {
            Context.ResultsManager.DeleteScreenDto(key);
            Destroy(element.gameObject);
        }

        private void ToggleElement(string key, bool isSelected)
        {
            Context.ResultsManager.ToggleScreenSelection(key, isSelected);
        }

        private void HandleStageCompleted(string message)
        {
            View.LogMessage(message);
        }

        private void OnDestroy()
        {
            Context.ImageProcessingManager.OnStageCompleted -= HandleStageCompleted;
        }
    }
}