using System;
using System.Collections.Generic;
using OpenCvSharp;
using UnityEngine;
using Rect = OpenCvSharp.Rect;

namespace Analysis
{
    public class TextureEntropyProcessor : MonoBehaviour
    { 

        private const float TargetEntropy = 2.0f;
        private const float EntropyTolerance = 0.5f; // Tolerancja dla wartości entropii 
        public List<Rect> ProcessTexture(Texture2D inputTexture)
        {
            var image = ImageProcessingUtils.TextureToMat(inputTexture);
            List<Rect> finalRects = new List<Rect>();
            DivideAndConquer(image, new Rect(0, 0, image.Width, image.Height), finalRects);

            return finalRects;
            // Wyświetl prostokąty lub wykonaj inne operacje
            foreach (var rect in finalRects)
            {
                Debug.Log($"Final rect: {rect}");
                Cv2.Rectangle(image, new OpenCvSharp.Point(rect.X, rect.Y),
                              new OpenCvSharp.Point(rect.X + rect.Width, rect.Y + rect.Height),
                              new Scalar(0, 0, 255), 2); // Czerwony kolor (BGR)
            }

            // Przekształć wynikowy obraz z powrotem do tekstury
            Texture2D outputTexture = ImageProcessingUtils.MatToTexture(image);
            GetComponent<Renderer>().material.mainTexture = outputTexture;
        }

        private void DivideAndConquer(Mat image, Rect rect, List<Rect> finalRects)
        {
            var roi = new Mat(image, rect);
            float entropy = GetHistogramEntropy(roi);

            if (Math.Abs(entropy - TargetEntropy) <= EntropyTolerance || (rect.Width <= 10 || rect.Height <= 10))
            {
                finalRects.Add(rect);
                return;
            }

            if (rect.Width >= rect.Height)
            {
                int halfWidth = rect.Width / 2;
                DivideAndConquer(image, new Rect(rect.X, rect.Y, halfWidth, rect.Height), finalRects);
                DivideAndConquer(image, new Rect(rect.X + halfWidth, rect.Y, rect.Width - halfWidth, rect.Height), finalRects);
            }
            else
            {
                int halfHeight = rect.Height / 2;
                DivideAndConquer(image, new Rect(rect.X, rect.Y, rect.Width, halfHeight), finalRects);
                DivideAndConquer(image, new Rect(rect.X, rect.Y + halfHeight, rect.Width, rect.Height - halfHeight), finalRects);
            }
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
    }
}
