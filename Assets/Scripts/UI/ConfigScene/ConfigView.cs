using System;
using Analysis;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.MVC;

namespace UI.ConfigScene
{
    public class ConfigView : BaseMainView
    {
        public event Action UploadButtonClicked;
        public event Action AnaliseButtonClicked;
        public event Action SettingsButtonClicked;
        public event Action<string, ScreenElement> RemoveElementClicked;
        public event Action<string, bool> ToggleElementClicked;
        public AnalysisSettingsElement AnalysisSettingsElement => analysisSettingsElement;

        [SerializeField]
        private ScreenElement filePrefab;

        [SerializeField]
        private Transform parentTransform;

        [SerializeField]
        private Button uploadButton;

        [SerializeField]
        private Button analiseButton;

        [SerializeField]
        private Button settingsButton;

        [SerializeField]
        private GameObject loadingOverlay;

        [SerializeField]
        private TMP_Text logText;

        [SerializeField]
        private AnalysisSettingsElement analysisSettingsElement;

        protected override void RegisterEvents()
        {
            base.RegisterEvents();
            uploadButton.onClick.AddListener(OnUploadButtonClick);
            analiseButton.onClick.AddListener(OnAnaliseButtonClicked);
            settingsButton.onClick.AddListener(() => SettingsButtonClicked?.Invoke());
        }

        protected override void UnregisterEvents()
        {
            base.UnregisterEvents();
            uploadButton.onClick.RemoveListener(OnUploadButtonClick);
            analiseButton.onClick.RemoveListener(OnAnaliseButtonClicked);
            settingsButton.onClick.RemoveAllListeners();
        }

        public void DisplayFiles(ScreenModel[] screenDataArray)
        {
            foreach (Transform child in parentTransform)
            {
                Destroy(child.gameObject);
            }

            foreach (var file in screenDataArray)
            {
                DisplayFile(file);
            }
        }

        public void DisplayFile(ScreenModel screenModel)
        {
            var fileObject = Instantiate(filePrefab, parentTransform);
            fileObject.Setup(screenModel);
            fileObject.OnDelete += (key, element) => RemoveElementClicked?.Invoke(key, element);
            fileObject.OnToggle += (key, isEnabled) => ToggleElementClicked?.Invoke(key, isEnabled);
        }

        private void OnUploadButtonClick()
        {
            UploadButtonClicked?.Invoke();
        }

        private void OnAnaliseButtonClicked()
        {
            loadingOverlay.SetActive(true);
            logText.text = "";
            AnaliseButtonClicked?.Invoke();
        }

        public void LogMessage(string message)
        {
            logText.text = message + "\n" + logText.text;
        }
    }
}