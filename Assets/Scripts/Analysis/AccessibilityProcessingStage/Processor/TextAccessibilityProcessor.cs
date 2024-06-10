using System.Collections.Generic;
using Analysis.PropertiesProcessingStage.Processor;
using OpenCvSharp;
using UnityEngine;
using Point = OpenCvSharp.Point;

namespace Analysis.AccessibilityProcessingStage.Processor
{
    public class TextAccessibilityProcessor
    {
        private readonly ColorPropertiesProcessor _colorPropertiesProcessor;

        //to config
        private readonly int _fontSizeThreshold = 14;
        private readonly float _imageAsBackgroundThresholdProbability = 0.3f;
        private readonly float _minimalDistanceBetweenLines = 1.5f;
        private readonly float _contrastLowerThreshold = 4.5f;
        private readonly float _contrastUpperThreshold = 15.5f;

        private readonly Color _defaultColor = Color.red;


        public TextAccessibilityProcessor(ColorPropertiesProcessor colorPropertiesProcessor)
        {
            _colorPropertiesProcessor = colorPropertiesProcessor;
        }

        public List<WordModel> TooSmallContrastBetweenTextAndBackground(List<WordModel> model)
        {
            return model.FindAll(m =>
                _colorPropertiesProcessor.CalculateContrastRatio(m.TextColor, m.TextBackgroundColor) <
                _contrastLowerThreshold);
        }

        public List<WordModel> TooHighContrastBetweenTextAndBackground(List<WordModel> model)
        {
            return model.FindAll(m =>
                _colorPropertiesProcessor.CalculateContrastRatio(m.TextColor, m.TextBackgroundColor) >
                _contrastUpperThreshold);
        }

        public List<WordModel> WordsWithTooSmallFont(List<WordModel> model)
        {
            return model.FindAll(m => m.FontSize < _fontSizeThreshold);
        }

        public List<TextBlockModel> BlocksWithTooSmallInterlines(List<TextBlockModel> models)
        {
            return models.FindAll(CheckIfDistanceBetweenLinesIsTooSmall);
        }

        public bool CheckIfDistanceBetweenLinesIsTooSmall(TextBlockModel model)
        {
            return model.AverageFontSize * _minimalDistanceBetweenLines > model.AverageInterline;
        }

        public List<TextBlockModel> TextInBlockAlignmentToRight(List<TextBlockModel> models)
        {
            return models.FindAll(m => m.IsLeftAligned == false);
        }

        public List<TextBlockModel> BlocksWithImageAsBackground(List<TextBlockModel> models)
        {
            return models.FindAll(m =>
                ImageAsBackgroundInBlockPercentage(m) > _imageAsBackgroundThresholdProbability);
        }

        private float ImageAsBackgroundInBlockPercentage(TextBlockModel model)
        {
            var wordsWithImageAsBackground = ImageAsBackground(model.GetWords());
            var area = model.Box.w * model.Box.h;
            return AreaPercentage(wordsWithImageAsBackground, area);
        }

        private List<WordModel> ImageAsBackground(List<WordModel> model)
        {
            return model.FindAll(m => !m.IsUniformBackground);
        }

        public float AreaPercentage(IEnumerable<ITextModel> models, int imageArea)
        {
            var areaCount = 0;
            foreach (var textModel in models)
            {
                areaCount += textModel.Box.w * textModel.Box.h;
            }

            return areaCount / (float)imageArea * 100f;
        }

        public Mat DrawAreas(IEnumerable<ITextModel> areas, Mat mat, Color customColor = default)
        {
            var result = mat.Clone();
            var color = customColor == default ? _defaultColor : customColor;
            var colorScalar = new Scalar(color.b * 255, color.g * 255, color.r * 255); // Convert Unity color to BGR

            foreach (var area in areas)
            {
                var left = area.Box.x;
                var top = area.Box.y;
                var right = area.Box.x + area.Box.w;
                var bottom = area.Box.y + area.Box.h;

                if (right > left && bottom > top)
                {
                    Cv2.Rectangle(result, new Point(left, top), new Point(right, bottom), colorScalar, 2);
                }
            }

            return result;
        }
    }
}