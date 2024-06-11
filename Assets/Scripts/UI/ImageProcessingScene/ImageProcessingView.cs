using System;
using Analysis;
using UnityEngine;
using UnityEngine.UI;
using Utils.MVC;

namespace UI
{
    public class ImageProcessingView : BaseMainView
    {
        public event Action BackButtonClicked;

        [SerializeField]
        private Button backButton;

        [SerializeField]
        private RawImage originalImage;

        [SerializeField]
        private RawImage processedImage;


        public void Setup(ScreenForAnalysisSo screenForAnalysisData)
        {
            originalImage.texture = screenForAnalysisData.texture;
        }


        public void ShowProcessedImage(Texture2D preview)
        {
            processedImage.texture = preview;
        }

        protected override void RegisterEvents()
        {
            base.RegisterEvents();
            backButton.onClick.AddListener(() => BackButtonClicked?.Invoke());
        }

        protected override void UnregisterEvents()
        {
            base.UnregisterEvents();
            backButton.onClick.RemoveAllListeners();
        }
    }
}