using System;
using System.Diagnostics;
using System.IO;
using Manager;
using UnityEngine;
using Utils.MVC;
using Debug = UnityEngine.Debug;

namespace UI.ImageProcessingScene
{
    public class ImageProcessingController : BaseController<ImageProcessingView, ImageProcessingContext>
    {
        private int _currentScreenIndex = 0;

        public override void Setup()
        {
            base.Setup();
            SetupView();
        }

        protected override void CreateDispatcher()
        {
            base.CreateDispatcher();
            View.BackButtonClicked += Back;
            View.OpenReportClicked += OpenReportFile;
            View.OpenReportLocationClicked += OpenReportLocation;
            View.NextResultButtonClicked += NextResult;
            View.PreviousResultButtonClicked += PreviousResult;
        }

        private void PreviousResult()
        {
            _currentScreenIndex = Mathf.Max(0, _currentScreenIndex - 1);
            SetupView();
        }

        private void NextResult()
        {
            _currentScreenIndex = Mathf.Min(DataManager.AnalysisModel.Count - 1, _currentScreenIndex + 1);
            SetupView();
        }

        private void Back()
        {
            Context.ChangeScene("Scenes/ConfigScene");
        }


        private void SetupView()
        {
            var model = DataManager.AnalysisModel[_currentScreenIndex];
            View.Setup(model);
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
            var filePath = DataManager.AnalysisModel[_currentScreenIndex].ScreenData.pathToReport;
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