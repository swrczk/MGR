using System;
using System.Collections.Generic;
using OpenCvSharp;
using UnityEngine;
using Rect = OpenCvSharp.Rect;

namespace Analysis
{
    //done - but not working
    public class TextureContrastProcessor : MonoBehaviour
    {
        [SerializeField]
        private float contrastLowerThreshold = 4.5f;

        [SerializeField]
        private float contrastUpperThreshold = 15.0f;


        public (List<ContrastAnalysisResult> analysisResults, Texture2D outputTexture) CheckContrastInBlocks(List<Segment> segments, Texture2D inputTexture)
        {
            var analysisResults = new List<ContrastAnalysisResult>();
            var image = AnalysisUtils.TextureToMat(inputTexture);
            var outputImage = AnalysisUtils.MatToGrayScale(image.Clone(), true);
            foreach (var segment in segments)
            {
                var segmentResult = Process(image, segment, outputImage);
                analysisResults.Add(segmentResult);
            }

            var outputTexture = AnalysisUtils.MatToTexture(outputImage);
            return (analysisResults, outputTexture);
        }

        private ContrastAnalysisResult Process(Mat image, Segment segment, Mat outputImage)
        {
            var block = new Mat(image, segment.Rect);
            var overallContrast = CalculateOverallContrast(block);

            if ((overallContrast < contrastLowerThreshold))
            {
                // Oznacz blok czerwonym kwadratem
                var rectColor = new Scalar(0, 0, 255);
                DrawTransparentRectangle(segment.Rect, outputImage, rectColor);
                return new ContrastAnalysisResult(segment,ContrastResultType.Low, overallContrast);
            }

            if (overallContrast > contrastUpperThreshold)
            {
                // Oznacz blok niebieskim kwadratem
                var rectColor = new Scalar(255, 0, 0);
                DrawTransparentRectangle(segment.Rect, outputImage, rectColor);
                return new ContrastAnalysisResult(segment,ContrastResultType.High, overallContrast);
            }
            return new ContrastAnalysisResult(segment,ContrastResultType.Normal, overallContrast);
        }

        private void DrawTransparentRectangle(Rect rect, Mat image, Scalar color, float opacity = 0.5f)
        {
            var roi = new Mat(image, rect);
            var overlay = new Mat(roi.Size(), roi.Type(), color);
            Cv2.AddWeighted(overlay, opacity, roi, 1 - opacity, 0, roi);
        }

        private Vec3d GetAverageColor(Mat block)
        {
            var averageScalar = Cv2.Mean(block);
            return new Vec3d(averageScalar.Val0, averageScalar.Val1, averageScalar.Val2);
        }


        //values from 1 to 21
        private float CalculateOverallContrast(Mat roi)
        {
            var gray = AnalysisUtils.MatToGrayScale(roi);

            // Znajdź najjaśniejszy i najciemniejszy piksel w wycinku
            double minVal, maxVal;
            Cv2.MinMaxLoc(gray, out minVal, out maxVal);

            // Konwersja najjaśniejszego i najciemniejszego piksela na Vec3b (zakładamy, że są to składowe RGB)
            var minColor = new Vec3b((byte)minVal, (byte)minVal, (byte)minVal);
            var maxColor = new Vec3b((byte)maxVal, (byte)maxVal, (byte)maxVal);

            return CalculateContrastRatio(maxColor, minColor);
        }


        //do obliczania kontrastu tekstu z tłem
        //values from 1 to 21
        public float CalculateContrastRatio(Vec3b color1, Vec3b color2)
        {
            var luminance1 = CalculateLuminance(color1);
            var luminance2 = CalculateLuminance(color2);

            // Upewnij się, że luminance1 jest większe niż luminance2
            if (luminance1 < luminance2)
            {
                (luminance1, luminance2) = (luminance2, luminance1);
            }

            return (luminance1 + 0.05f) / (luminance2 + 0.05f);
        }

        //values from 0 to 1
        private float CalculateLuminance(Vec3b color)
        {
            // Normalizuj wartości RGB do zakresu 0-1
            var r = color.Item2 / 255.0f;
            var g = color.Item1 / 255.0f;
            var b = color.Item0 / 255.0f;

            // Skoryguj wartości RGB
            var rPrime = (float)((r <= 0.03928) ? r / 12.92 : Math.Pow((r + 0.055) / 1.055, 2.4));
            var gPrime = (float)((g <= 0.03928) ? g / 12.92 : Math.Pow((g + 0.055) / 1.055, 2.4));
            var bPrime = (float)((b <= 0.03928) ? b / 12.92 : Math.Pow((b + 0.055) / 1.055, 2.4));

            // Oblicz luminancję
            return 0.2126f * rPrime + 0.7152f * gPrime + 0.0722f * bPrime;
        }
    }
}