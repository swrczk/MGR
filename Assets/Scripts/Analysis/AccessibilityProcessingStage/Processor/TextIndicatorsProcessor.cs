using System.Collections.Generic;
using Analysis.AccessibilityProcessingStage.Processor;
using Analysis.PropertiesProcessingStage.Model;
using OpenCvSharp;
using UI;
using UnityEngine;

namespace Analysis.AccessibilityProcessingStage
{
    public class TextIndicatorsProcessor
    {
        private readonly TextAccessibilityProcessor _textAccessibilityProcessor;
        private readonly AnalysisParametersSO _parameters;


        public TextIndicatorsProcessor(AnalysisParametersSO analysisParametersSo,
            TextAccessibilityProcessor textAccessibilityProcessor)
        {
            _textAccessibilityProcessor = textAccessibilityProcessor;
            _parameters = analysisParametersSo;
        }

        public TextAccessibilityModel GenerateTextAccessibilityModel(PropertiesModel model, Mat originalMat)
        {
            var words = model.DocumentModelProperties
                .GetWords();
            var textBlocks = model.DocumentModelProperties.TextBlocks;

            return new TextAccessibilityModel()
            {
                HighContrastTextColors = HighContrastTextColorsIndicator(words, originalMat),
                LowContrastTextColors = LowContrastTextColorsIndicator(words, originalMat),
                TooSmallFontSize = TooSmallFontIndicator(words, originalMat),
                ImageAsTextBackground =
                    ImageAsBackgroundIndicator(textBlocks, originalMat),
                RightAlignedText = RightAlignmentTextIndicator(textBlocks, originalMat),
                TooSmallInterlineInTextBlock =
                    TooSmallInterlineInTextBlockIndicator(textBlocks, originalMat),
            };
        }

        private AccessibilityIndicatorModel TooSmallInterlineInTextBlockIndicator(List<TextBlockModel> models, Mat mat)
        {
            var color = Color.red;
            var items = _textAccessibilityProcessor.BlocksWithTooSmallInterlines(models);
            return CalculateTextPropertyIndex(items, mat, color, FeatureType.TooSmallInterlineInTextBlock);
        }

        private AccessibilityIndicatorModel HighContrastTextColorsIndicator(List<WordModel> models, Mat mat)
        {
            var color = Color.red;
            var items = _textAccessibilityProcessor.TooHighContrastBetweenTextAndBackground(models);
            return CalculateTextPropertyIndex(items, mat, color, FeatureType.HighTextBackgroundContrast);
        }

        private AccessibilityIndicatorModel RightAlignmentTextIndicator(List<TextBlockModel> models, Mat mat)
        {
            var color = Color.red;
            var items = _textAccessibilityProcessor.TextInBlockAlignmentToRight(models);
            return CalculateTextPropertyIndex(items, mat, color, FeatureType.RightAlignedText);
        }

        private AccessibilityIndicatorModel LowContrastTextColorsIndicator(List<WordModel> models, Mat mat)
        {
            var color = Color.blue;

            var items = _textAccessibilityProcessor.TooSmallContrastBetweenTextAndBackground(models);
            return CalculateTextPropertyIndex(items, mat, color, FeatureType.LowTextBackgroundContrast);
        }

        private AccessibilityIndicatorModel TooSmallFontIndicator(List<WordModel> models, Mat mat)
        {
            var color = Color.red;

            var items = _textAccessibilityProcessor.WordsWithTooSmallFont(models);
            return CalculateTextPropertyIndex(items, mat, color, FeatureType.TooSmallFont);
        }

        private AccessibilityIndicatorModel ImageAsBackgroundIndicator(List<TextBlockModel> models, Mat mat)
        {
            var color = Color.red;
            var items = _textAccessibilityProcessor.BlocksWithImageAsBackground(models);
            return CalculateTextPropertyIndex(items, mat, color, FeatureType.ImageAsTextBackground);
        }

        private AccessibilityIndicatorModel CalculateTextPropertyIndex(IEnumerable<ITextModel> models, Mat mat,
            Color color, FeatureType featureType)
        {
            var imageArea = mat.Width * mat.Height;
            var areaPercentage = _textAccessibilityProcessor.AreaPercentage(models, imageArea);
            var visualization = _textAccessibilityProcessor.DrawAreas(models, mat, color);
            Debug.Log(
                $"Area percentage: {areaPercentage}, weight: {_parameters.GetWeight(featureType)}, type: {featureType}");
            return new AccessibilityIndicatorModel()
            {
                BaseValue = areaPercentage,
                Weight = _parameters.GetWeight(featureType),
                Visualization = visualization
            };
        }
    }
}