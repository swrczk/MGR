using Analysis.AccessibilityProcessingStage.Model;
using Analysis.PreprocessingStage.Model;
using Analysis.PropertiesProcessingStage.Model;

namespace Analysis.ReportGenerationStage.Model
{
    public class AnalysisModel
    {
        public ScreenModel ScreenData { get; set; }
        public AnalysisParametersSO AnalysisParametersSo { get; set; }
        public PreprocessedModel PreprocessedModel { get; set; }
        public PropertiesModel PropertiesModel { get; set; }
        public AccessibilityModel AccessibilityModel { get; set; }
        public ImageProcessingParametersSo ImageProcessingParameters { get; set; }
    }
}