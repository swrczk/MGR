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

        [SerializeField]
        private Button backButton;

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
        }

        protected override void UnregisterEvents()
        {
            base.UnregisterEvents();
            backButton.onClick.RemoveAllListeners();
        }
    }
}