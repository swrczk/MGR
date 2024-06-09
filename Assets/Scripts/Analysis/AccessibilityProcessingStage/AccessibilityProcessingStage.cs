using Analysis.AccessibilityProcessingStage.Processor;
using Analysis.PropertiesProcessingStage.Model;
using Analysis.PropertiesProcessingStage.Processor;
using OpenCvSharp;

namespace Analysis.AccessibilityProcessingStage
{
    public class AccessibilityModel
    {
        public ColorAccessibilityModel ColorAccessibilityModel { get; set; }
        public TextAccessibilityModel TextAccessibilityModel { get; set; }
    }

    public class ColorAccessibilityModel
    {
        public AccessibilityIndicatorModel DominantColorsContrast { get; set; }
    }

    public class AccessibilityProcessingStage
    {
        private readonly ColorPropertiesProcessor _colorPropertiesProcessor;
        private readonly TextAccessibilityProcessor _textAccessibilityProcessor;
        private readonly ColorAccessibilityProcessor _colorAccessibilityProcessor;
        private readonly AnalysisParametersSO _parameters;
        private readonly TextIndicatorsProcessor _textIndicatorsProcessor;

        public AccessibilityProcessingStage(AnalysisParametersSO parameters)
        {
            _parameters = parameters;
            _colorPropertiesProcessor = new ColorPropertiesProcessor();
            _textAccessibilityProcessor = new TextAccessibilityProcessor(_colorPropertiesProcessor);
            _colorAccessibilityProcessor = new ColorAccessibilityProcessor();
            _textIndicatorsProcessor = new TextIndicatorsProcessor(_parameters, _textAccessibilityProcessor);
        }

        public AccessibilityModel Run(PropertiesModel model, Mat originalMat)
        {
            return new AccessibilityModel
            {
                // ColorAccessibilityModel = GenerateColorAccessibilityModel(model, originalMat),
                TextAccessibilityModel = _textIndicatorsProcessor.GenerateTextAccessibilityModel(model, originalMat)
            };
        }
    }
}