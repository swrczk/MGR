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

        private ScreenModel _screenModelData;
        private PreprocessingStage _preprocessingStage;
        private PropertiesProcessingStage _propertiesProcessingStage;
        private AccessibilityProcessingStage _accessibilityProcessingStage;
        private ReportGenerationStage _reportGenerationStage;


        public async UniTask AnaliseRefactored(ScreenModel screen)
        {
            InitStages();
            await UniTask.WaitForFixedUpdate();

            _screenModelData = screen;
            var texture = _screenModelData.texture;

            var preprocessedModel = await _preprocessingStage.Run(texture);
            _screenModelData.SaveDominantColors(preprocessedModel.DominantColors);

            var propertiesModel = _propertiesProcessingStage.Run(preprocessedModel, texture);
            
            var accessibilityModel = _accessibilityProcessingStage.Run(propertiesModel, preprocessedModel.DefaultMat,
                preprocessedModel.ColorSegmentedMat);
            
            var analysisModel = new AnalysisModel()
            {
                ScreenData = _screenModelData,
                AnalysisParametersSo = analysisParametersSo,
                ImageProcessingParameters = imageProcessingParametersSo,
                PreprocessedModel = preprocessedModel,
                PropertiesModel = propertiesModel,
                AccessibilityModel = accessibilityModel
            };
            DataManager.AnalysisModel = analysisModel;
            _reportGenerationStage.Run(analysisModel);
            
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