using System;
using Analysis;
using UnityEngine;
using UnityEngine.UI;
using Utils.MVC;

namespace UI.ConfigScene
{
    public class ConfigView : BaseMainView
    {
        public event Action UploadButtonClicked;
        public event Action AnaliseButtonClicked;
        public event Action<string, ScreenElement> RemoveElementClicked;
        
        [SerializeField]
        private ScreenElement filePrefab;

        [SerializeField]
        private Transform parentTransform;

        [SerializeField]
        private Button uploadButton;

        [SerializeField]
        private Button analiseButton;
        
        [SerializeField]
        private GameObject loadingOverlay;

        protected override void RegisterEvents()
        {
            base.RegisterEvents();
            uploadButton.onClick.AddListener(OnUploadButtonClick);
            analiseButton.onClick.AddListener(OnAnaliseButtonClicked);
        }

        protected override void UnregisterEvents()
        {
            base.UnregisterEvents();
            uploadButton.onClick.RemoveListener(OnUploadButtonClick);
            analiseButton.onClick.RemoveListener(OnAnaliseButtonClicked);
        }

        public void DisplayFiles(ScreenForAnalysisSo[] screenDataArray)
        {
            foreach (Transform child in parentTransform)
            {
                Destroy(child.gameObject);
            }

            foreach (ScreenForAnalysisSo file in screenDataArray)
            {
                DisplayFile(file);
            }
        }

        public void DisplayFile(ScreenForAnalysisSo screenForAnalysisSo)
        {
            var fileObject = Instantiate(filePrefab, parentTransform);
            fileObject.Setup(screenForAnalysisSo);
            fileObject.OnDelete += (key, element) => RemoveElementClicked?.Invoke(key, element);
        }

        private void OnUploadButtonClick()
        {
            UploadButtonClicked?.Invoke();
        }

        private void OnAnaliseButtonClicked()
        {
            loadingOverlay.SetActive(true);
            AnaliseButtonClicked?.Invoke();
        }
    }
}