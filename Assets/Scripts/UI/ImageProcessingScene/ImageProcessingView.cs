using System;
using Analysis;
using Analysis.ReportGenerationStage.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.MVC;

namespace UI.ImageProcessingScene
{
    public class ImageProcessingView : BaseMainView
    {
        public event Action BackButtonClicked;
        public event Action OpenReportClicked;
        public event Action OpenReportLocationClicked;
        public event Action NextResultButtonClicked;
        public event Action PreviousResultButtonClicked;

        [SerializeField]
        private Button backButton;

        [SerializeField]
        private Button nextResultButton;

        [SerializeField]
        private Button previousResultButton;

        [SerializeField]
        private Button openReportButton;

        [SerializeField]
        private Button openReportLocation;

        [SerializeField]
        private RawImage originalImage;

        [SerializeField]
        private TMP_Text textScore;

        [SerializeField]
        private TMP_Text colorScore;

        [SerializeField]
        private TMP_Text overallScore;

        public void Setup(AnalysisModel model)
        {
            originalImage.texture = model.PreprocessedModel.DefaultMat.ToTexture();
            textScore.text = $"{model.AccessibilityModel.TextScore}";
            colorScore.text = $"{model.AccessibilityModel.ColorScore}";
            overallScore.text = $"{model.AccessibilityModel.OverallScore}";
        }

        protected override void RegisterEvents()
        {
            base.RegisterEvents();
            backButton.onClick.AddListener(() => BackButtonClicked?.Invoke());
            openReportButton.onClick.AddListener(() => OpenReportClicked?.Invoke());
            openReportLocation.onClick.AddListener(() => OpenReportLocationClicked?.Invoke());
            nextResultButton.onClick.AddListener(() => NextResultButtonClicked?.Invoke());
            previousResultButton.onClick.AddListener(() => PreviousResultButtonClicked?.Invoke());
        }

        protected override void UnregisterEvents()
        {
            base.UnregisterEvents();
            backButton.onClick.RemoveAllListeners();
            openReportButton.onClick.RemoveAllListeners();
            openReportLocation.onClick.RemoveAllListeners();
            nextResultButton.onClick.RemoveAllListeners();
            previousResultButton.onClick.RemoveAllListeners();
        }
    }
}