using System.Collections.Generic;
using System.Linq;
using OpenCvSharp;
using UnityEngine;

namespace Analysis
{
    public class TextPropertiesProcessor : MonoBehaviour
    {
        public void DetectTextColor(ScreenForAnalysisSo screenForAnalysisData)
        {
            var texture = screenForAnalysisData.ProcessedImages[ImageProcessingType.ColorSegmentation];
            var image = ImageProcessingUtils.TextureToMat(texture);
            var wordBoxes = screenForAnalysisData.textDocument.GetWords();
            foreach (var word in wordBoxes)
            {
                var colors = ColorsSortedByFrequency(image, word.Box);

                var color0 = Vec3ToColor(colors[0]);
                var color1 = Vec3ToColor(colors[1]);
                
                //for single letter text color is dominant color, for longer text background color is dominant
                
                bool isUniformBackground = IsUniformBackground(image, word.Box); 
                if (word.Text.Length == 1)
                {
                    word.SaveTextColor(color0);
                    word.SaveBackgroundColor(color1, isUniformBackground);
                }
                else
                {
                    word.SaveTextColor(color1);
                    word.SaveBackgroundColor(color0, isUniformBackground);
                }

                Debug.Log(
                    $"[TextPropertiesProcessor] GetDominantColor ({word.Text}): \n TextColor: {word.TextColor} - TextBackgroundColor: {word.TextBackgroundColor} - IsUniformBackground: {word.IsUniformBackground}");
            }
        }

        private static Color Vec3ToColor(Vec3b dominantColor)
        {
            return new Color(dominantColor.Item0, dominantColor.Item1, dominantColor.Item2);
        }

        private List<Vec3b> ColorsSortedByFrequency(Mat image, Box box)
        {
            // Wycięcie obszaru boxu z obrazu
            var roi = new Mat(image, new OpenCvSharp.Rect(box.x, box.y, box.w, box.h));

            // Konwersja obrazu na listę pikseli
            var pixels = new List<Vec3b>();
            for (var y = 0; y < roi.Rows; y++)
            {
                for (var x = 0; x < roi.Cols; x++)
                {
                    pixels.Add(roi.At<Vec3b>(y, x));
                }
            }

            var dominantColors = GetColors(pixels);
            return dominantColors.OrderByDescending(x => x.Value).Select(kv => kv.Key).ToList();
        }
        private bool IsUniformBackground(Mat image, Box box)
        {
            // Wycięcie obszaru boxu z obrazu
            Mat roi = new Mat(image, new OpenCvSharp.Rect(box.x, box.y, box.w, box.h));

            // Konwersja obrazu na skalę szarości
            Mat gray = new Mat();
            Cv2.CvtColor(roi, gray, ColorConversionCodes.RGB2GRAY);


            // Analiza odchylenia standardowego kolorów
            Cv2.MeanStdDev(gray, out Scalar mean, out Scalar stdDev);
            double variance = stdDev.Val0 * stdDev.Val0;


            // Użycie filtru Sobela do wykrywania krawędzi
            Mat sobelX = new Mat();
            Mat sobelY = new Mat();
            Cv2.Sobel(gray, sobelX, MatType.CV_64F, 1, 0, ksize: 5);
            Cv2.Sobel(gray, sobelY, MatType.CV_64F, 0, 1, ksize: 5);
            Mat sobel = new Mat();
            Cv2.Magnitude(sobelX, sobelY, sobel);

            // Próg krawędzi
            Mat edges = new Mat();
            Cv2.Threshold(sobel, edges, 50, 255, ThresholdTypes.Binary);

            // Analiza liczby krawędzi
            int numEdges = Cv2.CountNonZero(edges);

            // Ustalanie progów dla jednolitego tła (wartości do dostrojenia)
            double varianceThreshold = 500;
            int edgesThreshold = 100;

            // Ocena jednolitości tła
            return variance < varianceThreshold && numEdges < edgesThreshold;
        }

        private static Dictionary<Vec3b, int> GetColors(List<Vec3b> pixels)
        {
            var colorCounts = new Dictionary<Vec3b, int>(new Vec3BEqualityComparer());
            foreach (var pixel in pixels)
            {
                if (colorCounts.ContainsKey(pixel))
                {
                    colorCounts[pixel]++;
                }
                else
                {
                    colorCounts[pixel] = 1;
                }
            }

            return colorCounts;
        }

        private class Vec3BEqualityComparer : IEqualityComparer<Vec3b>
        {
            public bool Equals(Vec3b x, Vec3b y)
            {
                return x.Item0 == y.Item0 && x.Item1 == y.Item1 && x.Item2 == y.Item2;
            }

            public int GetHashCode(Vec3b obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}