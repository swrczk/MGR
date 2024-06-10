using Analysis.AccessibilityProcessingStage.Processor;
using Analysis.PropertiesProcessingStage.Model;
using Analysis.PropertiesProcessingStage.Processor;
using OpenCvSharp;

namespace Analysis.AccessibilityProcessingStage
{
    public class AccessibilityProcessingStage
    {
        private readonly ColorPropertiesProcessor _colorPropertiesProcessor;
        private readonly TextAccessibilityProcessor _textAccessibilityProcessor;
        private readonly ColorAccessibilityProcessor _colorAccessibilityProcessor;
        private readonly AnalysisParametersSO _parameters;
        private readonly TextIndicatorsProcessor _textIndicatorsProcessor;
        private readonly ColorIndicatorsProcessor _colorIndicatorsProcessor;

        public AccessibilityProcessingStage(AnalysisParametersSO parameters)
        {
            _parameters = parameters;
            _colorPropertiesProcessor = new ColorPropertiesProcessor();
            _textAccessibilityProcessor = new TextAccessibilityProcessor(_colorPropertiesProcessor);
            _colorAccessibilityProcessor = new ColorAccessibilityProcessor();
            _textIndicatorsProcessor = new TextIndicatorsProcessor(_parameters, _textAccessibilityProcessor);
            _colorIndicatorsProcessor = new ColorIndicatorsProcessor(_parameters, _colorAccessibilityProcessor);
        }

        public AccessibilityModel Run(PropertiesModel model, Mat originalMat)
        {
            var textAccessibilityModel = _textIndicatorsProcessor.GenerateTextAccessibilityModel(model, originalMat);
            var colorAccessibilityModel = _colorIndicatorsProcessor.GenerateColorAccessibilityModel(model, originalMat);
            return new AccessibilityModel
            {
                ColorAccessibilityModel = colorAccessibilityModel,
                TextAccessibilityModel = textAccessibilityModel
            };
        }
    }
}