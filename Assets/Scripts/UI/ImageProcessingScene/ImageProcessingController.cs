using System;
using System.Diagnostics;
using System.IO;
using Manager;
using Utils.MVC;
using Debug = UnityEngine.Debug;

namespace UI.ImageProcessingScene
{
    public class ImageProcessingController : BaseController<ImageProcessingView, ImageProcessingContext>
    {
        public override void Setup()
        {
            base.Setup();
            var model = DataManager.AnalysisModel;
            View.Setup(model);
        }

        protected override void CreateDispatcher()
        {
            base.CreateDispatcher();
            View.BackButtonClicked += Back;
            View.OpenReportClicked += OpenReportFile;
            View.OpenReportLocationClicked += OpenReportLocation;
        }

        private void Back()
        {
            Context.ChangeScene("Scenes/ConfigScene");
        }


        private void OpenReportLocation()
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var directoryPath = Path.Combine(documentsPath, "AccessibilityAnalysisRaports");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            OpenDirectoryInExplorer(directoryPath);
        }

        private void OpenDirectoryInExplorer(string directoryPath)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            Process.Start("explorer.exe", directoryPath);
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        Process.Start("open", directoryPath);
#elif UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
        Process.Start("xdg-open", directoryPath);
#else
        Debug.LogWarning("Opening directories in File Explorer is not supported on this platform.");
#endif
        }


        private void OpenReportFile()
        {
            var filePath = DataManager.AnalysisModel.ScreenData.pathToReport;
            if (File.Exists(filePath))
            {
                OpenFileInDefaultApplication(filePath);
            }
            else
            {
                Debug.LogWarning("Report file does not exist.");
            }
        }

        private void OpenFileInDefaultApplication(string filePath)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        Process.Start("open", filePath);
#elif UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
        Process.Start("xdg-open", filePath);
#else
        Debug.LogWarning("Opening files is not supported on this platform.");
#endif
        }
    }
}