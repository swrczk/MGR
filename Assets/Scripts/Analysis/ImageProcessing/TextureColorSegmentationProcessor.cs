using System;
using System.Collections.Generic;
using OpenCvSharp;
using UnityEngine;
using Rect = OpenCvSharp.Rect;

namespace Analysis
{
    public class TextureColorSegmentationProcessor : MonoBehaviour
    {
        private const float TargetColorAmount = 3;
        private const float ColorThreshold = 50;
        private const float EntropyThreshold = 0.3f;

        public (List<Rect> finalRects, Texture2D outputTexture) ProcessTexture(Texture2D inputTexture,
            bool excludeWords = false, List<Word> ocrWords = null)
        {
            var image = ImageProcessingUtils.TextureToMat(inputTexture);
            var finalRects = new List<Rect>();
            DivideAndConquer(image, new Rect(0, 0, image.Width, image.Height), finalRects);
            if (excludeWords && ocrWords != null)
                finalRects = ExcludeRectsOverlappingWithWords(finalRects, ocrWords);

            // Wyświetl prostokąty lub wykonaj inne operacje
            foreach (var rect in finalRects)
            {
                Cv2.Rectangle(image, new OpenCvSharp.Point(rect.X, rect.Y),
                    new OpenCvSharp.Point(rect.X + rect.Width, rect.Y + rect.Height),
                    new Scalar(0, 0, 255), 2); // Czerwony kolor (BGR)
            }

            // Przekształć wynikowy obraz z powrotem do tekstury
            var outputTexture = ImageProcessingUtils.MatToTexture(image);
            return (finalRects, outputTexture);
        }

        private void DivideAndConquer(Mat image, Rect rect, List<Rect> finalRects)
        {
            var roi = new Mat(image, rect);
            var entropy = GetHistogramEntropy(roi);
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
                DivideAndConquer(image, new Rect(rect.X, rect.Y, halfWidth, rect.Height), finalRects);
                DivideAndConquer(image, new Rect(rect.X + halfWidth, rect.Y, rect.Width - halfWidth, rect.Height),
                    finalRects);
            }
            else
            {
                var halfHeight = rect.Height / 2;
                DivideAndConquer(image, new Rect(rect.X, rect.Y, rect.Width, halfHeight), finalRects);
                DivideAndConquer(image, new Rect(rect.X, rect.Y + halfHeight, rect.Width, rect.Height - halfHeight),
                    finalRects);
            }
        }

        private int GetDistinctColorCount(Mat image)
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

        private double ColorDistanceSqr(Vec3b color1, Vec3b color2)
        {
            double deltaL = color1.Item0 - color2.Item0;
            double deltaA = color1.Item1 - color2.Item1;
            double deltaB = color1.Item2 - color2.Item2;

            return deltaL * deltaL + deltaA * deltaA + deltaB * deltaB;
        }


        private static float GetHistogramEntropy(Mat roi)
        {
            var gray = ImageProcessingUtils.MatToGreyScale(roi);
            // Analiza histogramu kolorów
            var hist = new Mat();
            int[] histSize = { 256 };
            Rangef[] ranges = { new Rangef(0, 256) };
            Cv2.CalcHist(new Mat[] { gray }, new int[] { 0 }, null, hist, 1, histSize, ranges);

            // Normalizacja histogramu
            Cv2.Normalize(hist, hist, 1.0, 0.0, NormTypes.L1);

            // Obliczenie entropii histogramu
            var entropy = 0.0f;
            for (var i = 0; i < hist.Rows; i++)
            {
                var binVal = hist.At<float>(i);
                if (binVal > 0)
                {
                    entropy -= binVal * (float)Math.Log(binVal, 2); // log base 2
                }
            }

            return entropy;
        }

        private List<Rect> ExcludeRectsOverlappingWithWords(List<Rect> rects, List<Word> ocrWords)
        {
            var filteredRects = new List<Rect>();

            foreach (var rect in rects)
            {
                var overlaps = false;
                foreach (var word in ocrWords)
                {
                    var wordRect = new Rect(word.Box.x, word.Box.y, word.Box.w, word.Box.h);
                    if (rect.IntersectsWith(wordRect))
                    {
                        overlaps = true;
                        break;
                    }
                }

                if (!overlaps)
                {
                    filteredRects.Add(rect);
                }
            }

            return filteredRects;
        }
    }
}