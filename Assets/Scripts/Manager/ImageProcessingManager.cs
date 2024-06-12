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
        [SerializeField]
        private ImageProcessingParametersSo imageProcessingParametersSo;

        [FormerlySerializedAs("analysisParameters")]
        [SerializeField]
        private AnalysisParametersSO analysisParametersSo;

        private ScreenModel _screenModel;
        private PreprocessingStage _preprocessingStage;
        private PropertiesProcessingStage _propertiesProcessingStage;
        private AccessibilityProcessingStage _accessibilityProcessingStage;
        private ReportGenerationStage _reportGenerationStage;


        public async UniTask AnaliseRefactored(ScreenModel screen)
        {
            InitStages();
            await UniTask.WaitForFixedUpdate();

            _screenModel = screen;

            var preprocessedModel = await _preprocessingStage.Run(_screenModel.texture);
            _screenModel.SaveDominantColors(preprocessedModel.DominantColors);

            var propertiesModel = _propertiesProcessingStage.Run(preprocessedModel, _screenModel.texture);

            var accessibilityModel = _accessibilityProcessingStage.Run(propertiesModel, preprocessedModel.DefaultMat,
                preprocessedModel.ColorSegmentedMat);
            _screenModel.overallScore = accessibilityModel.OverallScore;
            _screenModel.textScore = accessibilityModel.TextScore;
            _screenModel.colorScore = accessibilityModel.ColorScore;

            var analysisModel = new AnalysisModel()
            {
                ScreenData = _screenModel,
                AnalysisParametersSo = analysisParametersSo,
                ImageProcessingParameters = imageProcessingParametersSo,
                PreprocessedModel = preprocessedModel,
                PropertiesModel = propertiesModel,
                AccessibilityModel = accessibilityModel
            };
            var reportPath = _reportGenerationStage.Run(analysisModel);
            analysisModel.ScreenData.pathToReport = reportPath;
            DataManager.AnalysisModel = analysisModel;
        }

        private void InitStages()
        {
            _preprocessingStage = new PreprocessingStage(imageProcessingParametersSo);
            _propertiesProcessingStage = new PropertiesProcessingStage(imageProcessingParametersSo);
            _accessibilityProcessingStage = new AccessibilityProcessingStage(analysisParametersSo);
            _reportGenerationStage = new ReportGenerationStage();
        }
    }
}