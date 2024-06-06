using System.Collections.Generic;
using OpenCvSharp;
using Rect = OpenCvSharp.Rect;

namespace Analysis
{
    public class SegmentationProcessor
    {
        //move to config
        private const float TargetColorAmount = 3;
        private const float ColorThreshold = 50;
        private const float EntropyThreshold = 0.3f;

        public SegmentedTexture FindSegments(Mat mat)
        {
            var finalRects = new List<Rect>();
            var rect = new Rect(0, 0, mat.Width, mat.Height);
            DivideAndConquer(mat, rect, finalRects);
            return CreateSegments(mat, finalRects, rect);
        }

        private Mat GetSegmentationTexture(Mat mat, List<Segment> finalSegments)
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
                var entropy = AnalysisUtils.GetHistogramEntropy(roi, convertToGreyScale:true);
                var colorCount = GetDistinctColorCount(roi);
                if (colorCount <= TargetColorAmount || entropy < EntropyThreshold ||
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

        private SegmentedTexture CreateSegments(Mat mat, List<Rect> rects, Rect imageRect)
        {
            var segments = new List<Segment>();
            float imageArea = imageRect.Width * imageRect.Height;

            foreach (var rect in rects)
            {
                float rectArea = rect.Width * rect.Height;
                float volumePercentage = (rectArea / imageArea) * 100;
                segments.Add(new Segment { Rect = rect, VolumePercentage = volumePercentage });
            }

            var outputTexture = GetSegmentationTexture(mat, segments);

            return new SegmentedTexture(imageRect, segments, outputTexture);
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
                    if (ColorDistanceSqr(color, distinctColor) < ColorThreshold * ColorThreshold)
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

        public SegmentedTexture ExcludeRectsOverlappingWithWords(Mat mat, SegmentedTexture originalSegmentation,
            List<WordModel> ocrWords)
        {
            var filteredRects = new List<Segment>();
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

            return new SegmentedTexture(originalSegmentation.ImageRect, filteredRects, outputTexture);
        }
    }
}