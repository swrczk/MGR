using System;
using System.Collections.Generic;
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
                    Process(image, blockRect,  outputImage);
                }
            }
            Texture2D outputTexture = ImageProcessingUtils.MatToTexture(outputImage);
            return outputTexture;
        }

        private void Process(Mat image, Rect blockRect,   Mat outputImage)
        {
            var block = new Mat(image, blockRect);

            var averageColorVector = GetAverageColor(block);
            var averageColor = ImageProcessingUtils.Vec3ToColor(averageColorVector);

            var contrast = CalculateContrast(block);
            var overallContrast = CalculateOverallContrast(block);
            var entropy = GetHistogramEntropy(block);
            var averageColorLuminance = CalculateLuminance(ImageProcessingUtils.Vec3dToVec3b(averageColorVector));

            // Debug.Log($"[TextureContrastProcessor] Average color of block ({i}, {j}): {color}");
            Debug.Log($"[TextureContrastProcessor] block ( : contrast {contrast}, entropy: {entropy},\n " +
                      $" overallContrast: {overallContrast},  averageColorLuminance : {averageColorLuminance} ");


            // var tooHigh = "TOO HIGH";
            // var tooLow = "TOO LOW";
            if ((overallContrast < contrastThreshold )&&  averageColorLuminance < 0.2) 
            { 
                var rectColor = new Scalar(0, 0, 255);
                DrawTransparentRectangle(blockRect, outputImage, rectColor);
            } 
            if (overallContrast > 18f && averageColorLuminance > 0.8f && averageColor != Color.white) 
            { 
                var rectColor = new Scalar(255,0, 0); 
                DrawTransparentRectangle(blockRect, outputImage, rectColor);
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
        private void DrawTransparentRectangle( Rect rect, Mat image,Scalar color, float opacity = 0.5f)
        {
            // Pobierz region zainteresowania (ROI)
            var roi = new Mat(image, rect);

            // Stwórz obraz prostokąta o tych samych wymiarach co ROI
            var overlay = new Mat(roi.Size(), roi.Type(), color);

            // Nakładanie prostokąta z przezroczystością
            Cv2.AddWeighted(overlay, opacity, roi, 1 - opacity, 0, roi);
        }
        
        private static void DrawRect(Rect blockRect, Mat outputImage, Scalar rectColor)
        {
            var p1 = new OpenCvSharp.Point(blockRect.X, blockRect.Y);
            var p2 = new OpenCvSharp.Point(blockRect.X + blockRect.Width, blockRect.Y + blockRect.Height);
            Cv2.Rectangle(outputImage, p1,p2, rectColor, -1);  
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
    }
}