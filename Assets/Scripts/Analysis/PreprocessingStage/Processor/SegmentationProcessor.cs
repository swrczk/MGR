using System.Collections.Generic;
using Analysis.PreprocessingStage.Model;
using Analysis.PreprocessingStage.Model.TextModels;
using OpenCvSharp;
using Rect = OpenCvSharp.Rect;

namespace Analysis.PreprocessingStage.Processor
{
    public class SegmentationProcessor
    {
        
        private readonly float _targetColorAmount;
        private readonly float _colorThreshold;
        private readonly float _entropyThreshold;

        public SegmentationProcessor(float targetColorAmount, float colorThreshold, float entropyThreshold)
        {
            _targetColorAmount = targetColorAmount;
            _colorThreshold = colorThreshold;
            _entropyThreshold = entropyThreshold;
        }

        public SegmentedTextureModel FindSegments(Mat mat)
        {
            var finalRects = new List<Rect>();
            var rect = new Rect(0, 0, mat.Width, mat.Height);
            DivideAndConquer(mat, rect, finalRects);
            return CreateSegments(mat, finalRects, rect);
        }

        private Mat GetSegmentationTexture(Mat mat, List<SegmentModel> finalSegments)
        {
            var segmentedTexture = mat.Clone();
            foreach (var segment in finalSegments)
            {
                Cv2.Rectangle(segmentedTexture, new Point(segment.Rect.X, segment.Rect.Y),
                    new Point(segment.Rect.X + segment.Rect.Width, segment.Rect.Y + segment.Rect.Height),
                    new Scalar(0, 0, 255), 2);
            }

            return segmentedTexture;
        }

        private void DivideAndConquer(Mat mat, Rect rect, List<Rect> finalRects)
        {
            while (true)
            {
                var roi = new Mat(mat, rect);
                var entropy = AnalysisUtils.GetHistogramEntropy(roi, convertToGreyScale: true);
                var colorCount = GetDistinctColorCount(roi);
                if (colorCount <= _targetColorAmount || entropy < _entropyThreshold ||
                    (rect.Width <= 10 || rect.Height <= 10))
                {
                    finalRects.Add(rect);
                    return;
                }

                if (rect.Width >= rect.Height)
                {
                    var halfWidth = rect.Width / 2;
                    DivideAndConquer(mat, new Rect(rect.X, rect.Y, halfWidth, rect.Height), finalRects);
                    rect = new Rect(rect.X + halfWidth, rect.Y, rect.Width - halfWidth, rect.Height);
                }
                else
                {
                    var halfHeight = rect.Height / 2;
                    DivideAndConquer(mat, new Rect(rect.X, rect.Y, rect.Width, halfHeight), finalRects);
                    rect = new Rect(rect.X, rect.Y + halfHeight, rect.Width, rect.Height - halfHeight);
                }
            }
        }

        private SegmentedTextureModel CreateSegments(Mat mat, List<Rect> rects, Rect imageRect)
        {
            var segments = new List<SegmentModel>();
            float imageArea = imageRect.Width * imageRect.Height;

            foreach (var rect in rects)
            {
                float rectArea = rect.Width * rect.Height;
                float volumePercentage = (rectArea / imageArea) * 100;
                segments.Add(new SegmentModel { Rect = rect, VolumePercentage = volumePercentage });
            }

            var outputTexture = GetSegmentationTexture(mat, segments);

            return new SegmentedTextureModel(imageRect, segments, outputTexture);
        }

        private int GetDistinctColorCount(Mat image)
        {
            var uniqueColors = GetUniqueColors(image);

            // Grupowanie kolorów, które są percepcyjnie blisko siebie
            var distinctColors = new List<Vec3b>();
            foreach (var color in uniqueColors)
            {
                var isDistinct = true;
                foreach (var distinctColor in distinctColors)
                {
                    if (ColorDistanceSqr(color, distinctColor) < _colorThreshold * _colorThreshold)
                    {
                        isDistinct = false;
                        break;
                    }
                }

                if (isDistinct)
                {
                    distinctColors.Add(color);
                }
            }

            return distinctColors.Count;
        }

        private static HashSet<Vec3b> GetUniqueColors(Mat image)
        {
            // Konwersja obrazu do przestrzeni kolorów Lab
            var labImage = new Mat();
            Cv2.CvtColor(image, labImage, ColorConversionCodes.BGR2Lab);

            // Zbieranie unikalnych kolorów w przestrzeni Lab
            var uniqueColors = new HashSet<Vec3b>(new Vec3BEqualityComparer());
            for (var y = 0; y < labImage.Rows; y++)
            {
                for (var x = 0; x < labImage.Cols; x++)
                {
                    uniqueColors.Add(labImage.At<Vec3b>(y, x));
                }
            }

            return uniqueColors;
        }

        private double ColorDistanceSqr(Vec3b color1, Vec3b color2)
        {
            double deltaL = color1.Item0 - color2.Item0;
            double deltaA = color1.Item1 - color2.Item1;
            double deltaB = color1.Item2 - color2.Item2;

            return deltaL * deltaL + deltaA * deltaA + deltaB * deltaB;
        }

        public SegmentedTextureModel ExcludeRectsOverlappingWithWords(Mat mat,
            SegmentedTextureModel originalSegmentation,
            List<WordModel> ocrWords)
        {
            var filteredRects = new List<SegmentModel>();
            foreach (var segment in originalSegmentation.Segments)
            {
                var overlaps = false;
                foreach (var word in ocrWords)
                {
                    var wordRect = new Rect(word.Box.x, word.Box.y, word.Box.w, word.Box.h);
                    if (segment.Rect.IntersectsWith(wordRect))
                    {
                        overlaps = true;
                        break;
                    }
                }

                if (!overlaps)
                {
                    filteredRects.Add(segment);
                }
            }

            var outputTexture = GetSegmentationTexture(mat, filteredRects);

            return new SegmentedTextureModel(originalSegmentation.ImageRect, filteredRects, outputTexture);
        }
    }
}