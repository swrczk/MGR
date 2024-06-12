using Analysis.AccessibilityProcessingStage.Processor;
using Analysis.PropertiesProcessingStage.Model;
using Analysis.PropertiesProcessingStage.Processor;
using OpenCvSharp;
using UnityEngine;

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

        private readonly int _roundDigits = 3;

        public AccessibilityProcessingStage(AnalysisParametersSO parameters)
        {
            _parameters = parameters;
            _colorPropertiesProcessor = new ColorPropertiesProcessor();
            _textAccessibilityProcessor = new TextAccessibilityProcessor(_colorPropertiesProcessor);
            _colorAccessibilityProcessor = new ColorAccessibilityProcessor();
            _textIndicatorsProcessor = new TextIndicatorsProcessor(_parameters, _textAccessibilityProcessor);
            _colorIndicatorsProcessor = new ColorIndicatorsProcessor(_parameters, _colorAccessibilityProcessor);
        }

        public AccessibilityModel Run(PropertiesModel model, Mat originalMat, Mat colorSegmentedMat)
        {
            var textAccessibilityModel = _textIndicatorsProcessor.GenerateTextAccessibilityModel(model, originalMat);
            var colorAccessibilityModel =
                _colorIndicatorsProcessor.GenerateColorAccessibilityModel(model, colorSegmentedMat);

            var colorScore = CalculateColorScore(colorAccessibilityModel);
            var textScore = CalculateTextScore(textAccessibilityModel);
            var overallScore = colorScore + textScore;

            Debug.Log($"Color score: {colorScore}, Text score: {textScore}, Overall score: {overallScore}");
            
            return new AccessibilityModel
            {
                ColorAccessibilityModel = colorAccessibilityModel,
                TextAccessibilityModel = textAccessibilityModel,
                OverallScore = overallScore,
                TextScore = textScore,
                ColorScore = colorScore
            };
        }

        private float CalculateTextScore(TextAccessibilityModel model)
        {
            var score = 0f;
            score += CalculateIndicatorScore(model.RightAlignedText);
            score += CalculateIndicatorScore(model.HighContrastTextColors);
            score += CalculateIndicatorScore(model.LowContrastTextColors);
            score += CalculateIndicatorScore(model.ImageAsTextBackground);
            score += CalculateIndicatorScore(model.TooSmallFontSize);
            score += CalculateIndicatorScore(model.TooSmallInterlineInTextBlock);

            return score;
        }

        private float CalculateColorScore(ColorAccessibilityModel model)
        {
            var score = 0f;
            foreach (var indicatorModel in model.DominantColorsWithHighContrast)
            {
                score += CalculateIndicatorScore(indicatorModel);
            }

            foreach (var indicatorModel in model.DominantColorsWithLowContrast)
            {
                score += CalculateIndicatorScore(indicatorModel);
            }

            score += CalculateIndicatorScore(model.SegmentsWithHighContrast);
            score += CalculateIndicatorScore(model.SegmentsWithLowContrast);
            return score;
        }

        private float CalculateIndicatorScore(AccessibilityIndicatorModel model)
        {
            // return (float)System.Math.Round(model.BaseValue * model.Weight, _roundDigits);
            return model.BaseValue * model.Weight;
        }
    }
}