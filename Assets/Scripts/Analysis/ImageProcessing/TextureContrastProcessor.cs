using System;
using System.Collections.Generic;
using System.Net;
using OpenCvSharp;
using UnityEngine;
using Rect = OpenCvSharp.Rect;

namespace Analysis
{
    public class TextureContrastProcessor : MonoBehaviour
    {
        [SerializeField]
        private int blockWidth = 100;

        [SerializeField]
        private int blockHeight = 100;

        [SerializeField]
        private double contrastThreshold = 4.5;

        [SerializeField]
        private int padding = 2;


//new
        [SerializeField]
        private double contrastLowerThreshold = 4.5;

        [SerializeField]
        private double contrastUpperThreshold = 15.0;

        [SerializeField]
        private double brightnessLowerThreshold = 50.0;

        [SerializeField]
        private double brightnessUpperThreshold = 200.0;

        [SerializeField]
        private double luminanceLowerThreshold = 0.2;

        [SerializeField]
        private double luminanceUpperThreshold = 0.8;

        public Texture2D CheckContrastInBlocks(List<Rect> rects, Texture2D inputTexture)
        {
            var image = ImageProcessingUtils.TextureToMat(inputTexture);
            Mat outputImage = ImageProcessingUtils.MatToGreyScale(image.Clone(), true);
            foreach (var rect in rects)
            {
                Process(image, rect, outputImage);
            }

            Texture2D outputTexture = ImageProcessingUtils.MatToTexture(outputImage);
            return outputTexture;
        }

        public Texture2D CheckContrastInBlocks(Texture2D inputTexture)
        {
            var image = ImageProcessingUtils.TextureToMat(inputTexture);

            // Liczba bloków w pionie i poziomie
            var numBlocksX = Mathf.FloorToInt(image.Width / (float)blockWidth);
            var numBlocksY = Mathf.FloorToInt(image.Height / (float)blockHeight);

            Mat outputImage = image.Clone();

            for (var i = 0; i < numBlocksX; i++)
            {
                for (var j = 0; j < numBlocksY; j++)
                {
                    var blockRect = new Rect(i * blockWidth, j * blockHeight, blockWidth, blockHeight);
                    Process(image, blockRect, outputImage);
                }
            }

            Texture2D outputTexture = ImageProcessingUtils.MatToTexture(outputImage);
            return outputTexture;
        }

        private void Process(Mat image, Rect blockRect, Mat outputImage)
        {
            var block = new Mat(image, blockRect);

            var averageColorVector = ImageProcessingUtils.Vec3dToVec3b(GetAverageColor(block));
            var averageColor = ImageProcessingUtils.Vec3ToColor(averageColorVector);
            //
            // var overallContrast = CalculateOverallContrast(block);
            // var averageColorLuminance = CalculateLuminance(ImageProcessingUtils.Vec3dToVec3b(averageColorVector));
            //

            var contrastResult = AnalyzeContrast(block);
            var brightnessResult = AnalyzeBrightness(block);
            var luminanceResult = AnalyzeLuminance(block);
            // Debug.Log($"[TextureContrastProcessor] Average color of block ({i}, {j}): {color}");
            // Debug.Log($"[TextureContrastProcessor] block ( : contrast {contrast}, entropy: {entropy},\n " +
            //           $" overallContrast: {overallContrast},  averageColorLuminance : {averageColorLuminance} ");


            // var tooHigh = "TOO HIGH";
            // var tooLow = "TOO LOW";
            var grey = ImageProcessingUtils.MatToGreyScale(image);
            var entropy = GetHistogramEntropy(grey);
            if (entropy <= 1f || ImageProcessingUtils.IsBigger(averageColorVector, new Vec3b(245, 245, 245)) ||
                ImageProcessingUtils.IsSmaller(averageColorVector, new Vec3b(20, 20, 20)))
            {
                return;
            }

            // var result = (  contrastResult +   luminanceResult) /2f;
            var result = (brightnessResult + contrastResult + luminanceResult)/3;

            if (result <-0.5f)
            {
                var rectColor = new Scalar(0, 0, 255);
                DrawTransparentRectangle(blockRect, outputImage, rectColor);
                Debug.Log(
                    $"[TextureContrastProcessor] Too low: average color:{averageColor}, contrast {contrastResult}, brightness {brightnessResult}, luminance {luminanceResult}, result {result}");
            }

            if (result >0.5)
            {
                var rectColor = new Scalar(255, 0, 0);
                DrawTransparentRectangle(blockRect, outputImage, rectColor);
                Debug.Log(
                    $"[TextureContrastProcessor] Too high: average color:{averageColor}, contrast {contrastResult}, brightness {brightnessResult}, luminance {luminanceResult}, result {result}");
            }

            // if ((overallContrast < contrastThreshold )&& entropy > 3.0f) 
            // {
            //     // Oznacz blok czerwonym kwadratem
            //     var rectColor = new Scalar(0, 0, 255);
            //     DrawTransparentRectangle(blockRect, outputImage, rectColor);
            // }
            //         
            // if (  averageColorLuminance < 0.2 && entropy > 1.0f) 
            // {
            //     // Oznacz blok czerwonym kwadratem
            //     var rectColor = new Scalar(0, 255, 0);
            //     DrawTransparentRectangle(blockRect, outputImage, rectColor);
            // }
            // if (overallContrast > 18f ) 
            // { 
            //     var rectColor = new Scalar(255,0, 0); 
            //     DrawTransparentRectangle(blockRect, outputImage, rectColor);
            // }
            // if (  averageColorLuminance > 0.8f && averageColor != Color.white ) 
            // { 
            //     var rectColor = new Scalar(255,255, 0);
            //     DrawTransparentRectangle(blockRect, outputImage, rectColor);
            // } 
        }

        private int AnalyzeContrast(Mat roi)
        {
            double contrast = CalculateOverallContrast(roi);

            if (contrast < contrastLowerThreshold)
                return -1;
            else if (contrast > contrastUpperThreshold)
                return 1;
            else
                return 0;
        }

        private int AnalyzeBrightness(Mat roi)
        {
            double brightness = CalculateAverageBrightness(roi);

            if (brightness < brightnessLowerThreshold)
                return -1;
            else if (brightness > brightnessUpperThreshold)
                return 1;
            else
                return 0;
        }

        private int AnalyzeLuminance(Mat roi)
        {
            double luminance = CalculateLuminance(roi);

            if (luminance < luminanceLowerThreshold)
                return -1;
            else if (luminance > luminanceUpperThreshold)
                return 1;
            else
                return 0;
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

        //values from 0 to 1
        private float CalculateContrast(Mat roi)
        {
            // Konwersja obrazu na skalę szarości
            var gray = ImageProcessingUtils.MatToGreyScale(roi);

            // Znajdź najjaśniejszy i najciemniejszy piksel
            double minVal, maxVal;
            Cv2.MinMaxLoc(gray, out minVal, out maxVal);

            // Oblicz współczynnik kontrastu
            var contrast = (float)((maxVal - minVal) / (maxVal + minVal));

            return contrast;
        }


        //values from 1 to 21
        private float CalculateOverallContrast(Mat roi)
        {
            // Konwersja obrazu na skalę szarości
            var gray = ImageProcessingUtils.MatToGreyScale(roi);

            // Znajdź najjaśniejszy i najciemniejszy piksel w wycinku
            double minVal, maxVal;
            Cv2.MinMaxLoc(gray, out minVal, out maxVal);

            // Konwersja najjaśniejszego i najciemniejszego piksela na Vec3b (zakładamy, że są to składowe RGB)
            var minColor = new Vec3b((byte)minVal, (byte)minVal, (byte)minVal);
            var maxColor = new Vec3b((byte)maxVal, (byte)maxVal, (byte)maxVal);

            // Oblicz współczynnik kontrastu
            var contrastRatio = CalculateContrastRatio(maxColor, minColor);

            return contrastRatio;
        }


        //do obliczania kontrastu tekstu z tłem
        //values from 1 to 21
        public float CalculateContrastRatio(Vec3b color1, Vec3b color2)
        {
            float luminance1 = CalculateLuminance(color1);
            float luminance2 = CalculateLuminance(color2);

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

        private double CalculateAverageBrightness(Mat roi)
        {
            Mat gray = new Mat();
            Cv2.CvtColor(roi, gray, ColorConversionCodes.BGR2GRAY);

            Scalar meanScalar = Cv2.Mean(gray);
            return meanScalar.Val0;
        }

        private double CalculateLuminance(Mat roi)
        {
            var averageColor = Cv2.Mean(roi);
            Vec3b color = new Vec3b((byte)averageColor[0], (byte)averageColor[1], (byte)averageColor[2]);
            return CalculateLuminance(color);
        } //Result Range <0;8>

        private static float GetHistogramEntropy(Mat gray)
        {
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
    }
}