using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using OpenCvSharp;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Analysis
{
    public class ColorDetectorOld : MonoBehaviour
    {
        [FormerlySerializedAs("screenData")]
        public ScreenForAnalysisSo screenForAnalysisData;
        public RawImage displayImage;
        public RawImage originalImage;
        public int clusterCount = 5;

    
        /// <summary>
        /// first version
        /// </summary>
        private void Start()
        {
            ApplyTexture(screenForAnalysisData.texture, originalImage);
            var dominantColors = DetectDominantColors(screenForAnalysisData.texture, clusterCount);
            var segmentation = SplitByColors(screenForAnalysisData.texture, dominantColors);
            screenForAnalysisData.SaveDominantColors(dominantColors);
            // screenForAnalysisData.SaveColorSegmentation(segmentation);
            ApplyTexture(segmentation, displayImage);
        }

        private List<Color> DetectDominantColors(Texture2D inputTexture, int clusterCount)
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

        private Texture2D SplitByColors(Texture2D inputTexture, List<Color> colors)
        {
            Mat img = TextureToMat(inputTexture);
            img.CvtColor(ColorConversionCodes.RGB2BGR);

            // Konwersja listy dominujących kolorów z Color na Vec3b
            List<Vec3b> bgrColors = new List<Vec3b>();
            foreach (Color color in colors)
            {
                bgrColors.Add(new Vec3b(
                    (byte)(color.b * 255),
                    (byte)(color.g * 255),
                    (byte)(color.r * 255)));
            }

            // Przypisanie każdego piksela do najbliższego koloru
            for (int y = 0; y < img.Rows; y++)
            {
                for (int x = 0; x < img.Cols; x++)
                {
                    Vec3b pixel = img.At<Vec3b>(y, x);
                    int closestColorIndex = -1;
                    double minDist = double.MaxValue;

                    for (int i = 0; i < bgrColors.Count; i++)
                    {
                        // Tworzenie macierzy dla obu kolorów, aby można było użyć Cv2.Norm
                        Mat pixelMat = new Mat(1, 1, MatType.CV_32FC3, new Scalar(pixel.Item0, pixel.Item1, pixel.Item2));
                        Mat colorMat = new Mat(1, 1, MatType.CV_32FC3, new Scalar(bgrColors[i].Item0, bgrColors[i].Item1, bgrColors[i].Item2));

                        // Obliczanie dystansu
                        double dist = Cv2.Norm(pixelMat, colorMat, NormTypes.L2);
                        if (dist < minDist)
                        {
                            minDist = dist;
                            closestColorIndex = i;
                        }
                    }

                    // Ustawienie piksela w obrazie wyjściowym na kolor najbliższego klastra
                    Vec3b closestColor = bgrColors[closestColorIndex];
                    img.Set(y, x, new Vec3b(closestColor[2], closestColor[1], closestColor[0]));
                }
            }

            // Konwersja Mat na Texture2D i wyświetlanie
            Texture2D outputTexture = MatToTexture(img);
            return outputTexture;
        }

        // Konwersja Mat do Texture2D
        private Texture2D MatToTexture(Mat mat)
        {
            Texture2D texture = new Texture2D(mat.Width, mat.Height, TextureFormat.RGB24, false);
            Mat rgbMat = new Mat();
            Cv2.CvtColor(mat, rgbMat, ColorConversionCodes.BGR2RGB);
            // Odwracanie obrazu w pionie
            Cv2.Flip(rgbMat, rgbMat, FlipMode.X);
            OpenCvSharp.Unity.MatToTexture(rgbMat, texture);
            return texture;
        }

        // Stosowanie tekstury do obiektu w scenie
        private void ApplyTexture(Texture2D texture, RawImage rawImage)
        {
            rawImage.texture = texture;
        }

        // Konwersja Texture2D do Mat
        private Mat TextureToMat(Texture2D texture)
        {
            Mat mat = new Mat(texture.height, texture.width, MatType.CV_8UC3);
            Color32[] colors = texture.GetPixels32();
            GCHandle handle = GCHandle.Alloc(colors, GCHandleType.Pinned);
            using (Mat temp = new Mat(texture.height, texture.width, MatType.CV_8UC4, handle.AddrOfPinnedObject()))
            {
                Cv2.CvtColor(temp, mat, ColorConversionCodes.BGRA2BGR);
            }

            handle.Free();
            return mat;
        }
    
        /// <summary>
        /// last version
        /// </summary>
        ///
    
    
        private Texture2D DetectAndSegmentColors(Texture2D inputTexture)
        {
            // Konwertuj Texture2D na OpenCvSharp Mat
            Mat img = OpenCvSharp.Unity.TextureToMat(inputTexture);

            // Algorytm k-means do wykrycia dominujących kolorów
            var dominantColors = DetectDominantColors(inputTexture, clusterCount);

            // Segmentacja krawędzi algorytmem Canny
            Mat edges = new Mat();
            Cv2.Canny(img, edges, 50, 150);

            // Znajdź kontury
            Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(edges, out contours, out hierarchy, RetrievalModes.Tree,
                ContourApproximationModes.ApproxSimple);

            // Utwórz maskę segmentacji
            Mat segmentationMask = Mat.Zeros(img.Size(), MatType.CV_8UC3);
            for (int i = 0; i < contours.Length; i++)
            {
                // Oblicz dominujący kolor dla danego obszaru
                Color dominantColor = GetDominantColorInContour(inputTexture, contours[i]);

                // Wypełnij obszar konturu kolorem dominującym
                Scalar colorScalar = new Scalar(dominantColor.r * 255, dominantColor.g * 255, dominantColor.b * 255);
                Cv2.DrawContours(segmentationMask, contours, i, colorScalar, -1);
            }

            // Konwersja Mat na Texture2D
            Texture2D outputTexture = MatToTexture(segmentationMask);
            return outputTexture;
        }
    
        private Texture2D ProcessImage2(Texture2D inputTexture) //crashing Unity!!!
        {
            // Convert Texture2D to Mat
            Mat img = OpenCvSharp.Unity.TextureToMat(inputTexture);
            Mat grayImg = new Mat();
            Cv2.CvtColor(img, grayImg, ColorConversionCodes.RGB2GRAY);

            // Detect edges for contour detection
            Mat edges = new Mat();
            Cv2.Canny(grayImg, edges, 100, 200); // Thresholds can be adjusted

            // Find contours
            Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(edges, out contours, out hierarchy, RetrievalModes.Tree,
                ContourApproximationModes.ApproxSimple);

            // Prepare a colored image to fill with the contours
            Mat coloredImage = Mat.Zeros(img.Size(), MatType.CV_8UC3);

            foreach (Point[] contour in contours)
            {
                // Calculate the dominant color or choose a color for the contour
                Scalar color = CalculateDominantColor(img, contour);
                Mat mask = Mat.Zeros(img.Size(), MatType.CV_8UC1);
                Cv2.DrawContours(mask, new Point[][] { contour }, -1, new Scalar(255), -1);

                // Fill the contour in the colored image using the mask
                coloredImage.SetTo(color, mask);
            }

            // Convert the filled Mat back to a Texture2D
            return MatToTexture(coloredImage);
        }

        Texture2D ProcessImage(Texture2D inputTexture)
        {
            // Convert Texture2D to Mat
            Mat image = OpenCvSharp.Unity.TextureToMat(inputTexture);
            Mat grayImage = new Mat();
            Cv2.CvtColor(image, grayImage, ColorConversionCodes.RGB2GRAY);

            // Detect edges
            Mat edges = new Mat();
            Cv2.Canny(grayImage, edges, 100, 200); // Adjust thresholds as necessary

            // Find contours
            Cv2.FindContours(edges, out Point[][] contours, out HierarchyIndex[] hierarchy, RetrievalModes.Tree,
                ContourApproximationModes.ApproxSimple);

            // Prepare an image to draw the filled contours
            Mat filledContours = Mat.Zeros(image.Size(), MatType.CV_8UC3);

            foreach (Point[] contour in contours)
            {
                // Calculate the dominant color for the given contour
                Scalar color = CalculateDominantColor(image, contour);

                // Fill the contour with the dominant color
                Cv2.DrawContours(filledContours, new Point[][] { contour }, -1, color, -1); // -1 fills the contour
            }

            // Convert back to Texture2D
            return MatToTexture(filledContours);
        }

        private Scalar CalculateDominantColor(Mat image, Point[] contour)
        {
            // Create a mask for the contour
            Mat mask = new Mat(image.Size(), MatType.CV_8UC1);
            Cv2.DrawContours(mask, new Point[][] { contour }, -1, new Scalar(255), -1);

            // Create a blank image to store the masked region
            Mat maskedImage = new Mat();
            image.CopyTo(maskedImage, mask);

            // Extract pixels from the masked image
            List<Vec3b> pixels = new List<Vec3b>();
            for (int x = 0; x < maskedImage.Width; x++)
            {
                for (int y = 0; y < maskedImage.Height; y++)
                {
                    // Check if the mask at this pixel is set
                    if (mask.At<byte>(y, x) == 255)
                    {
                        Vec3b pixel = maskedImage.At<Vec3b>(y, x);
                        // Check if the pixel is not black (assuming black is not a dominant color you're interested in)
                        if (!(pixel.Item0 == 0 && pixel.Item1 == 0 && pixel.Item2 == 0))
                        {
                            pixels.Add(pixel);
                        }
                    }
                }
            }

            // Find the most frequent color
            if (pixels.Count > 0)
            {
                var dominantColor = pixels.GroupBy(p => p)
                    .OrderByDescending(g => g.Count())
                    .First().Key;

                return new Scalar(dominantColor.Item0, dominantColor.Item1, dominantColor.Item2);
            }

            return new Scalar(0, 0, 0); // Return black if no other color is dominant or found
        }



        private Color GetDominantColorInContour(Texture2D inputTexture, Point[] contour)
        {
            // Utwórz listę kolorów wewnątrz konturu
            List<Color> colorsInsideContour = new List<Color>();
            foreach (var point in contour)
            {
                if (point.Y >= 0 && point.Y < inputTexture.height && point.X >= 0 && point.X < inputTexture.width)
                {
                    Color pixelColor = inputTexture.GetPixel((int)point.X, (int)point.Y);
                    colorsInsideContour.Add(pixelColor);
                }
            }

            // Znajdź najczęściej występujący kolor
            Color dominantColor = colorsInsideContour.GroupBy(x => x).OrderByDescending(g => g.Count()).Select(g => g.Key)
                .FirstOrDefault();

            return dominantColor;
        }
    }
} 