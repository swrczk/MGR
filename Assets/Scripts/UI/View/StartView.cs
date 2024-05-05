using System;
using Analysis;
using UI.Common;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class StartView : BaseMainView
    {
        public event Action UploadButtonClicked;
        public event Action<string, ScreenElement> RemoveElementClicked;

        [SerializeField]
        private ScreenElement filePrefab;

        [SerializeField]
        private Transform parentTransform;

        [SerializeField]
        private Button uploadButton;

        protected override void RegisterEvents()
        {
            base.RegisterEvents();
            uploadButton.onClick.AddListener(OnUploadButtonClick);
        }

        protected override void UnregisterEvents()
        {
            base.UnregisterEvents();
            uploadButton.onClick.RemoveListener(OnUploadButtonClick);
        }

        public void DisplayFiles(ScreenSo[] screenDataArray)
        {
            foreach (Transform child in parentTransform)
            {
                Destroy(child.gameObject);
            }

            foreach (ScreenSo file in screenDataArray)
            {
                DisplayFile(file);
            }
        }

        public void DisplayFile(ScreenSo screenSo)
        {
            var fileObject = Instantiate(filePrefab, parentTransform);
            fileObject.Setup(screenSo);
            fileObject.OnDelete += (key, element) => RemoveElementClicked?.Invoke(key, element);
        }

        private void OnUploadButtonClick()
        {
            UploadButtonClicked?.Invoke();
        }
    }
}