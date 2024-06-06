using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenCvSharp;
using UnityEngine;

namespace Analysis
{
    public class ColorProcessor
    {
        private Color _defaultContoursColor = Color.red;

        public Mat KMeansSegmentation(Mat mat, int k)
        {
            var img = mat.Clone();
            var imgReshaped = img.Reshape(1, img.Rows * img.Cols);
            var imgConverted = new Mat();
            imgReshaped.ConvertTo(imgConverted, MatType.CV_32F);

            var labels = new Mat();
            var centers = new Mat();

            var criteria = new TermCriteria(CriteriaType.Eps | CriteriaType.MaxIter, 10, 1.0);
            Cv2.Kmeans(imgConverted, k, labels, criteria, 10, KMeansFlags.PpCenters, centers);

            // Convert centers to uchar from float
            centers.ConvertTo(centers, MatType.CV_8UC1);
            centers = centers.Reshape(3, k);

            // Replace each pixel with its center value
            var result = new Mat(img.Size(), img.Type());
            var resultData = new byte[img.Rows * img.Cols * img.Channels()];
            var centersData = new byte[k * img.Channels()];
            Marshal.Copy(centers.Data, centersData, 0, k * img.Channels());
            var labelsData = new int[img.Rows * img.Cols];
            Marshal.Copy(labels.Data, labelsData, 0, img.Rows * img.Cols);

            for (var i = 0; i < labelsData.Length; i++)
            {
                var label = labelsData[i];
                var baseIndex = i * img.Channels();
                resultData[baseIndex + 0] = centersData[label * img.Channels() + 0]; // B
                resultData[baseIndex + 1] = centersData[label * img.Channels() + 1]; // G
                resultData[baseIndex + 2] = centersData[label * img.Channels() + 2]; // R
            }

            Marshal.Copy(resultData, 0, result.Data, resultData.Length);
            return result;
        }


        public Texture2D DrawContours(Mat mat, Point[][] contours, Color contourColor = default)
        {
            var color = AnalysisUtils.ColorToScalar(contourColor == default ? _defaultContoursColor : contourColor);

            var filledContours = mat.Clone();
            foreach (var contour in contours)
            {
                Cv2.DrawContours(filledContours, new Point[][] { contour }, -1, color);
            }

            return AnalysisUtils.MatToTexture(filledContours);
        }

        public (Mat edges, Point[][] contours) FindContours(Mat grayImage)
        {
            var edges = new Mat();
            Cv2.Canny(grayImage, edges, 50, 100);
            Cv2.FindContours(edges, out var contours, out var hierarchy, RetrievalModes.Tree,
                ContourApproximationModes.ApproxSimple);
            return (edges, contours);
        }

        public List<Color> GetDominantColors(Mat mat, int clusterCount)
        {
            var img = mat.Clone();
            img.CvtColor(ColorConversionCodes.RGB2HSV);

            // Zastosuj rozmycie Gaussa
            var blurredImg = new Mat();
            Cv2.GaussianBlur(img, blurredImg, new Size(5, 5), 0);

            // Przygotowanie danych do k-means
            var samples = blurredImg.Reshape(3, blurredImg.Rows * blurredImg.Cols);
            samples.ConvertTo(samples, MatType.CV_32F);

            // Parametry klasteryzacji
            var labels = new Mat();
            var centers = new Mat();
            var criteria = TermCriteria.Both(10, 1.0);

            // Klasteryzacja kolorów
            Cv2.Kmeans(samples, clusterCount, labels, criteria, 1, KMeansFlags.PpCenters, centers);

            var dominantColors = new List<Color>();
            // Wypisywanie środków klastrów jako kolorów
            for (var i = 0; i < clusterCount; i++)
            {
                var center = centers.At<Vec3f>(i);
                // Konwertuj HSV na RGB dla wizualizacji
                var colorHsv = new Scalar(center[0], center[1], center[2]);
                var color = ConvertColorHsvToRgb(colorHsv);
                dominantColors.Add(color);
            }

            return dominantColors;
        }

        private static Color ConvertColorHsvToRgb(Scalar colorHsv)
        {
            var colorMat = new Mat(1, 1, MatType.CV_32FC3, colorHsv);
            colorMat.ConvertTo(colorMat, MatType.CV_8UC3);
            colorMat.CvtColor(ColorConversionCodes.HSV2BGR);
            var colorRgb = colorMat.Get<Vec3b>(0, 0);
            var color = new Color(colorRgb[2] / 255f, colorRgb[1] / 255f, colorRgb[0] / 255f);
            return color;
        }
    }
}