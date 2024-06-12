using Analysis.AccessibilityProcessingStage;
using Analysis.PropertiesProcessingStage.Model;

namespace Analysis.ReportGenerationStage
{
    public class AnalysisModel
    {
        public ScreenForAnalysisSo ScreenData { get; set; }
        public AnalysisParametersSO AnalysisParameters { get; set; }
        public PreprocessedModel PreprocessedModel { get; set; }
        public PropertiesModel PropertiesModel { get; set; }
        public AccessibilityModel AccessibilityModel { get; set; }
        public ImageProcessingParametersSo ImageProcessingParameters { get; set; }
    }
}