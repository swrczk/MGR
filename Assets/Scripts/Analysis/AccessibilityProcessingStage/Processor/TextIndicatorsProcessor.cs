using System.Collections.Generic;
using System.Linq;
using Analysis.AccessibilityProcessingStage.Model;
using Analysis.PreprocessingStage.Model.TextModels;
using Analysis.PropertiesProcessingStage.Model;
using OpenCvSharp;
using UI;
using UnityEngine;

namespace Analysis.AccessibilityProcessingStage.Processor
{
    public class TextIndicatorsProcessor
    {
        private readonly TextAccessibilityProcessor _textAccessibilityProcessor;
        private readonly AnalysisParametersSO _parametersSo;


        public TextIndicatorsProcessor(AnalysisParametersSO analysisParametersSo,
            TextAccessibilityProcessor textAccessibilityProcessor)
        {
            _textAccessibilityProcessor = textAccessibilityProcessor;
            _parametersSo = analysisParametersSo;
        }

        public TextAccessibilityModel GenerateTextAccessibilityModel(PropertiesModel model, Mat originalMat)
        {
            var sumOfLetters = model.DocumentModelProperties.GetWords().Sum(w=>w.Text.Length+1);
            if (sumOfLetters < 20)
            {
                Debug.Log("Text is too small to analyze. Skipping text analysis.");
                return new TextAccessibilityModel()
                {
                    HighContrastTextColors = new AccessibilityIndicatorModel(),
                    LowContrastTextColors = new AccessibilityIndicatorModel(),
                    TooSmallFontSize =  new AccessibilityIndicatorModel(),
                    ImageAsTextBackground = new AccessibilityIndicatorModel(),
                    RightAlignedText =  new AccessibilityIndicatorModel(),
                    TooSmallInterlineInTextBlock = new AccessibilityIndicatorModel(),
                };
            }
            
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
            var areaPercentage = Mathf.Max(_textAccessibilityProcessor.AreaPercentage(models, imageArea), 0);
            var visualization = _textAccessibilityProcessor.DrawAreas(models, mat, color);
            Debug.Log(
                $"Area percentage: {areaPercentage}, weight: {_parametersSo.GetWeight(featureType)}, type: {featureType}");
            var model = new AccessibilityIndicatorModel()
            {
                BaseValue = areaPercentage,
                Weight = _parametersSo.GetWeight(featureType),
                Visualization = visualization
            };
            return model;
        }
    }
}