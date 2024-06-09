using System.Collections.Generic;
using System.Drawing;
using Analysis.PropertiesProcessingStage.Processor;
using OpenCvSharp;
using Point = OpenCvSharp.Point;

namespace Analysis.AccessibilityProcessingStage.Processor
{
    public class TextAccessibilityProcessor
    {
        private ColorPropertiesProcessor _colorPropertiesProcessor;

        //to config
        private int _fontSizeThreshold = 14;
        private float _imageAsBackgroundThresholdPropability = 0.3f;
        private float _minimalDistanceBetweenLines = 1.5f;
        private readonly float _contrastLowerThreshold = 4.5f;
        private readonly float _contrastUpperThreshold = 15.5f;


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

        public (float areaPercentage, int wordCount) SmallFontIndicator(List<WordModel> words, int imageArea)
        {
            var areaCount = 0;
            var count = 0;
            foreach (var word in words)
            {
                if (word.FontSize < _fontSizeThreshold)
                {
                    areaCount += word.Box.w * word.Box.h;
                    count++;
                }
            }

            return (areaCount / (float)imageArea, count);
        }

        public List<WordModel> WordsWithTooSmallFont(List<WordModel> model)
        {
            return model.FindAll(m => m.FontSize < _fontSizeThreshold);
        }

        public bool CheckDistanceBetweenLines(TextBlockModel model)
        {
            return model.AverageFontSize * _minimalDistanceBetweenLines < model.AverageInterline;
        }

        public List<TextBlockModel> TextInBlockAlignmentToRight(List<TextBlockModel> models)
        {
            return models.FindAll(m => m.IsLeftAligned == false);
        }

        public List<TextBlockModel> BlocksWithImageAsBackground(List<TextBlockModel> models)
        {
            return models.FindAll(m =>
                ImageAsBackgroundInBlockPercentage(m) > _imageAsBackgroundThresholdPropability);
        }

        public float ImageAsBackgroundInBlockPercentage(TextBlockModel model)
        {
            var wordsWithImageAsBackground = ImageAsBackground(model.GetWords());
            return AreaPercentage(wordsWithImageAsBackground, model.Box);
        }

        public List<WordModel> ImageAsBackground(List<WordModel> model)
        {
            return model.FindAll(m => !m.IsUniformBackground);
        }

        public float AreaPercentage(IEnumerable<ITextModel> models, Box area)
        {
            var imageArea = area.w * area.h;
            var areaCount = 0;
            foreach (var textModel in models)
            {
                areaCount += textModel.Box.w * textModel.Box.h;
            }

            return areaCount / (float)imageArea;
        }

        public Mat DrawAreas(List<Box> areas, Color color, Mat mat)
        {
            var result = mat.Clone();
            var colorScalar = new Scalar(color.R, color.G, color.G);
            foreach (var area in areas)
            {
                Cv2.Rectangle(result, new Point(area.x, area.y),
                    new Point(area.x + area.w, area.y + area.h),
                    colorScalar, 2);
            }

            return result;
        }
    }
}