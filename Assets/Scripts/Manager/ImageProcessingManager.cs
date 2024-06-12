using System;
using System.Collections.Generic;
using Analysis;
using Analysis.AccessibilityProcessingStage;
using Analysis.PreprocessingStage;
using Analysis.PropertiesProcessingStage;
using Analysis.ReportGenerationStage;
using Analysis.ReportGenerationStage.Model;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace Manager
{
    public class ImageProcessingManager : BaseManager
    {
        public event Action<string> OnStageCompleted;

        [SerializeField]
        private ImageProcessingParametersSo imageProcessingParametersSo;
        
        [SerializeField]
        private AnalysisParametersSO analysisParameters;

        private ScreenModel _screenModel;
        private PreprocessingStage _preprocessingStage;
        private PropertiesProcessingStage _propertiesProcessingStage;
        private AccessibilityProcessingStage _accessibilityProcessingStage;
        private ReportGenerationStage _reportGenerationStage;

        public AnalysisParametersSO AnalysisParameters => analysisParameters;

        public async UniTask AnaliseScreens(List<ScreenModel> screens)
        {
            InitStages();

            foreach (var screen in screens)
            {
                await UniTask.WaitForFixedUpdate();
                await AnaliseScreen(screen);
            }
        }

        private async UniTask AnaliseScreen(ScreenModel screen)
        {
            _screenModel = screen;
            OnStageCompleted?.Invoke($"\nStarting analysis process for screen: {screen.screenName} ... \n");
            OnStageCompleted?.Invoke("Starting preprocessing stage ...");
            await AllowUiToUpdate();

            var preprocessedModel = await _preprocessingStage.Run(_screenModel.texture);
            _screenModel.SaveDominantColors(preprocessedModel.DominantColors);
            OnStageCompleted?.Invoke("Preprocessing stage completed.");
            OnStageCompleted?.Invoke("Starting properties processing stage ...");
            await AllowUiToUpdate();

            var propertiesModel = _propertiesProcessingStage.Run(preprocessedModel, _screenModel.texture);
            OnStageCompleted?.Invoke("Properties processing stage completed.");
            OnStageCompleted?.Invoke("Starting accessibility processing stage ...");
            await AllowUiToUpdate();

            var accessibilityModel = _accessibilityProcessingStage.Run(propertiesModel, preprocessedModel.DefaultMat,
                preprocessedModel.ColorSegmentedMat);
            OnStageCompleted?.Invoke("Accessibility processing stage completed.");
            OnStageCompleted?.Invoke("Starting report generation stage ...");
            await AllowUiToUpdate();

            var analysisModel = new AnalysisModel()
            {
                ScreenData = _screenModel,
                AnalysisParametersSo = analysisParameters,
                ImageProcessingParameters = imageProcessingParametersSo,
                PreprocessedModel = preprocessedModel,
                PropertiesModel = propertiesModel,
                AccessibilityModel = accessibilityModel
            };
            var reportPath = _reportGenerationStage.Run(analysisModel);
            analysisModel.ScreenData.pathToReport = reportPath;
            DataManager.AnalysisModel.Add(analysisModel);

            OnStageCompleted?.Invoke("Report generation stage completed.");
            OnStageCompleted?.Invoke($"Analysis completed for screen: {screen.screenName}\n");
        }

        private static async UniTask AllowUiToUpdate()
        {
            await UniTask.SwitchToThreadPool();
            await UniTask.WaitForSeconds(1);
            await UniTask.SwitchToMainThread();
        }

        private void InitStages()
        {
            _preprocessingStage = new PreprocessingStage(imageProcessingParametersSo);
            _propertiesProcessingStage = new PropertiesProcessingStage(imageProcessingParametersSo);
            _accessibilityProcessingStage = new AccessibilityProcessingStage(analysisParameters);
            _reportGenerationStage = new ReportGenerationStage();
            DataManager.AnalysisModel.Clear();
        }
    }
}