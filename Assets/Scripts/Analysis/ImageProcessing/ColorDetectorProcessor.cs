using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenCvSharp;
using UnityEngine;

namespace Analysis
{
    public class ColorDetectorProcessor : MonoBehaviour
    {
        public Texture2D SegmentImageUsingKMeans(Texture2D inputTexture, int k)
        {
            Mat img = OpenCvSharp.Unity.TextureToMat(inputTexture);
            Mat imgReshaped = img.Reshape(1, img.Rows * img.Cols);
            Mat imgConverted = new Mat();
            imgReshaped.ConvertTo(imgConverted, MatType.CV_32F);

            Mat labels = new Mat();
            Mat centers = new Mat();
            // Correct use of TermCriteria with bitwise OR
            TermCriteria criteria = new TermCriteria(CriteriaType.Eps | CriteriaType.MaxIter, 10, 1.0);
            Cv2.Kmeans(imgConverted, k, labels, criteria, 10, KMeansFlags.PpCenters, centers);

            // Convert centers to uchar from float
            centers.ConvertTo(centers, MatType.CV_8UC1);
            centers = centers.Reshape(3, k);

            // Replace each pixel with its center value
            Mat result = new Mat(img.Size(), img.Type());
            byte[] resultData = new byte[img.Rows * img.Cols * img.Channels()];
            byte[] centersData = new byte[k * img.Channels()];
            Marshal.Copy(centers.Data, centersData, 0, k * img.Channels());
            int[] labelsData = new int[img.Rows * img.Cols];
            Marshal.Copy(labels.Data, labelsData, 0, img.Rows * img.Cols);

            for (int i = 0; i < labelsData.Length; i++)
            {
                int label = labelsData[i];
                int baseIndex = i * img.Channels();
                resultData[baseIndex + 0] = centersData[label * img.Channels() + 0]; // B
                resultData[baseIndex + 1] = centersData[label * img.Channels() + 1]; // G
                resultData[baseIndex + 2] = centersData[label * img.Channels() + 2]; // R
            }

            Marshal.Copy(resultData, 0, result.Data, resultData.Length);
            return OpenCvSharp.Unity.MatToTexture(result);
        }


        public Texture2D DrawContours(Texture2D inputTexture)
        {
            // Convert Texture2D to Mat
            Mat image = OpenCvSharp.Unity.TextureToMat(inputTexture);
            Mat grayImage = new Mat();
            Cv2.CvtColor(image, grayImage, ColorConversionCodes.RGB2GRAY);

            // Detect edges
            Mat edges = new Mat();
            Cv2.Canny(grayImage, edges, 50, 100); // Adjust thresholds as necessary

            // Find contours
            Cv2.FindContours(edges, out Point[][] contours, out HierarchyIndex[] hierarchy, RetrievalModes.Tree,
                ContourApproximationModes.ApproxSimple);

            // Prepare an image to draw the filled contours
            Mat filledContours = image.Clone();

            foreach (Point[] contour in contours)
            {
                // Calculate the dominant color for the given contour
                Scalar color = new Scalar(0, 0, 255);

                // Fill the contour with the dominant color
                Cv2.DrawContours(filledContours, new Point[][] { contour }, -1, color, 1); // -1 fills the contour
            }

            // Convert back to Texture2D
            return MatToTexture(filledContours);
        }

        public List<Color> DetectDominantColors(Texture2D inputTexture, int clusterCount)
        {
            // Konwertuj Texture2D na OpenCvSharp Mat
            Mat img = OpenCvSharp.Unity.TextureToMat(inputTexture);

            // Konwersja na przestrzeń kolorów HSV
            img.CvtColor(ColorConversionCodes.RGB2HSV);

            // Zastosuj rozmycie Gaussa
            Mat blurredImg = new Mat();
            Cv2.GaussianBlur(img, blurredImg, new Size(5, 5), 0);

            // Przygotowanie danych do k-means
            Mat samples = blurredImg.Reshape(3, blurredImg.Rows * blurredImg.Cols);
            samples.ConvertTo(samples, MatType.CV_32F);

            // Parametry klasteryzacji
            Mat labels = new Mat();
            Mat centers = new Mat();
            var criteria = TermCriteria.Both(10, 1.0);

            // Klasteryzacja kolorów
            Cv2.Kmeans(samples, clusterCount, labels, criteria, 1, KMeansFlags.PpCenters, centers);

            var dominantColors = new List<Color>();
            // Wypisywanie środków klastrów jako kolorów
            for (int i = 0; i < clusterCount; i++)
            {
                Vec3f center = centers.At<Vec3f>(i);
                // Konwertuj HSV na RGB dla wizualizacji
                Scalar colorHsv = new Scalar(center[0], center[1], center[2]);
                Mat colorMat = new Mat(1, 1, MatType.CV_32FC3, colorHsv);
                colorMat.ConvertTo(colorMat, MatType.CV_8UC3);
                colorMat.CvtColor(ColorConversionCodes.HSV2BGR);
                Vec3b colorRgb = colorMat.Get<Vec3b>(0, 0);
                Color color = new Color(colorRgb[2] / 255f, colorRgb[1] / 255f, colorRgb[0] / 255f);
                dominantColors.Add(color);
            }

            return dominantColors;
        }

        private Texture2D MatToTexture(Mat mat)
        {
            // Create a new Texture2D with the size of the mat and appropriate format.
            Texture2D texture = new Texture2D(mat.Width, mat.Height, TextureFormat.RGB24, false);

            // OpenCV typically uses BGR format, so convert it to RGB.
            Mat rgbMat = new Mat();
            Cv2.CvtColor(mat, rgbMat, ColorConversionCodes.BGR2RGB);

            // Unity reads textures with the bottom row first, and OpenCV is top row first,
            // so we need to flip it vertically.
            Mat flippedMat = new Mat();
            Cv2.Flip(rgbMat, flippedMat, FlipMode.X);

            // Convert the data to an array that can be handled by Unity Texture2D.
            var data = new byte[flippedMat.Rows * flippedMat.Cols * flippedMat.Channels()];
            Marshal.Copy(flippedMat.Data, data, 0, data.Length);

            texture.LoadRawTextureData(data);
            texture.Apply();

            return texture;
        }
    }
}