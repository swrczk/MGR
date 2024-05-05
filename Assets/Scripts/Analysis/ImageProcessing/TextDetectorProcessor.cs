using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using OpenCvSharp;
using UnityEngine;
using Rect = OpenCvSharp.Rect;

namespace Analysis
{
    public class TextDetectorProcessor : MonoBehaviour
    {
        public Texture2D Process(Texture2D texture)
        {
            Mat originalMat = TextureToMat(texture);
            var textRegions = ExtractTextRegions(originalMat);
            Mat colorMat = new Mat();
            Cv2.CvtColor(originalMat, colorMat, ColorConversionCodes.BGR2RGB); // Przekształcenie na kolorowy obraz RGB

            foreach (var rect in textRegions)
            {
                Cv2.Rectangle(colorMat, rect, new Scalar(0, 0, 255), 2); // Rysowanie czerwonego prostokąta
            }

            return MatToTexture(colorMat);
        }

        private Mat TextureToMat(Texture2D texture)
        {
            Color32[] pixels = texture.GetPixels32();
            Mat mat = new Mat(texture.height, texture.width, MatType.CV_8UC4, pixels);
            return mat;
        }

        private Texture2D MatToTexture(Mat mat)
        {
            // Sprawdzenie i przekonwertowanie mat do formatu CV_8UC4, jeśli to konieczne
            if (mat.Type() != MatType.CV_8UC4)
            {
                var tempMat = new Mat();
                if (mat.Channels() == 1)
                {
                    Cv2.CvtColor(mat, tempMat, ColorConversionCodes.GRAY2RGBA);
                }
                else if (mat.Channels() == 3)
                {
                    Cv2.CvtColor(mat, tempMat, ColorConversionCodes.BGR2RGBA);
                }
                else
                {
                    return null;
                    // throw new InvalidOperationException("Unsupported Mat format. Must have 1 or 3 channels.");
                }

                mat = tempMat;
            }

            // Tworzenie Texture2D
            Texture2D texture = new Texture2D(mat.Width, mat.Height, TextureFormat.RGBA32, false);

            // Odwracanie obrazu, ponieważ OpenCV używa innego układu koordynatów
            // Cv2.Flip(mat, mat, FlipMode.X);

            // Wczytywanie danych obrazu do tekstury
            texture.LoadRawTextureData(mat.Data, (int)(mat.Step() * mat.Rows));

            // Aplikacja zmian
            texture.Apply();

            return texture;
        }


        private Mat PreprocessImageToGreyscale(Mat mat)
        {
            //1st method
            // var grayMat = new Mat();
            // Cv2.CvtColor(mat, grayMat, ColorConversionCodes.BGR2GRAY);
            // Cv2.Threshold(grayMat, grayMat, 128, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);
            // return grayMat;


            //2nd method
            // var grayMat = new Mat();
            // Cv2.CvtColor(mat, grayMat, ColorConversionCodes.BGR2GRAY);
            // var blurred = new Mat();
            // Cv2.GaussianBlur(grayMat, blurred, new Size(5, 5), 0);
            // var adaptiveThresh = new Mat();
            // Cv2.AdaptiveThreshold(blurred, adaptiveThresh, 255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.BinaryInv, 11, 2);
            // return adaptiveThresh;

            //3rd method
            var grayMat = new Mat();
            Cv2.CvtColor(mat, grayMat, ColorConversionCodes.BGR2GRAY);
            var blurred = new Mat();
            Cv2.GaussianBlur(grayMat, blurred, new Size(5, 5), 0);
            var adaptiveThresh = new Mat();
            Cv2.AdaptiveThreshold(blurred, adaptiveThresh, 255, AdaptiveThresholdTypes.GaussianC,
                ThresholdTypes.BinaryInv, 11, 2);

            // Morfologiczne zamknięcie dla usunięcia małych elementów tła
            Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(3, 3));
            Cv2.MorphologyEx(adaptiveThresh, adaptiveThresh, MorphTypes.Close, kernel);

            return adaptiveThresh;
        }

        private Mat DetectEdges(Mat grayMat)
        {
            var edges = new Mat();
            Cv2.Sobel(grayMat, edges, MatType.CV_8U, 1, 0, 3); // X direction
            return edges;
        }

        private bool HasSerifs(Mat edges, Rect rect)
        {
            // Ekstrakcja regionu konturu
            Mat region = edges.SubMat(rect);

            // Analiza pikseli wzdłuż górnych i dolnych krawędzi regionu
            for (int x = 0; x < region.Width; x++)
            {
                // Sprawdzanie obecności serifików na górnej i dolnej krawędzi
                if (region.At<byte>(0, x) > 0 && region.At<byte>(region.Height - 1, x) > 0)
                    return true; // Serify zazwyczaj widoczne na krawędziach
            }

            return false;
        }

        private Mat[] FindContours(Mat mat)
        {
            Mat hierarchy = new Mat();
            Mat[] contours;
            Cv2.FindContours(mat, out contours, hierarchy, RetrievalModes.External,
                ContourApproximationModes.ApproxSimple);
            return contours;
        }

        // public Texture2D Process(Texture2D texture)
        // {
        //     Mat mat = TextureToMat(texture);
        //     var grayMat = PreprocessImageToGreyscale(mat);
        //     // var edges = DetectEdges(grayMat);
        //     var contours = FindContours(grayMat);
        //
        //     Mat colorEdges = new Mat();
        //     // Cv2.CvtColor(edges, colorEdges, ColorConversionCodes.GRAY2BGR);
        //     Cv2.CvtColor(grayMat, colorEdges, ColorConversionCodes.GRAY2BGR);
        //     foreach (var contour in contours)
        //     {
        //         Rect rect = Cv2.BoundingRect(contour);
        //         // if (HasSerifs(edges, rect))
        //         if (HasSerifs(grayMat, rect))
        //         {
        //             Debug.Log("Serif found in contour at: " + rect);
        //             // Obrysuj kontur czerwonym kolorem na obrazie 'edges'
        //             Cv2.Rectangle(colorEdges, rect, new Scalar(0, 0, 255), 2); // Użycie czerwonego koloru
        //         }
        //     }
        //
        //     return MatToTexture(colorEdges);
        // }

        // private List<Mat> ExtractTextRegions(Mat image)
        // {
        //     List<Mat> textRegions = new List<Mat>();
        //     Mat grayImage = new Mat();
        //     Cv2.CvtColor(image, grayImage, ColorConversionCodes.BGR2GRAY);
        //     Cv2.AdaptiveThreshold(grayImage, grayImage, 255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.Binary,
        //         11, 2);
        //
        //     Mat hierarchy = new Mat();
        //     Mat[] contours;
        //     Cv2.FindContours(grayImage, out contours, hierarchy, RetrievalModes.External,
        //         ContourApproximationModes.ApproxSimple);
        //
        //     foreach (Mat contour in contours)
        //     {
        //         Rect boundingBox = Cv2.BoundingRect(contour);
        //         if (boundingBox.Width > 10 && boundingBox.Height > 10) // Filtr minimalnego rozmiaru
        //         {
        //             Mat textRegion = new Mat(image, boundingBox);
        //             textRegions.Add(textRegion);
        //         }
        //     }
        //
        //     return textRegions;
        // }
        
        private List<Rect> ExtractTextRegions(Mat image)
        {
            List<Rect> textRegions = new List<Rect>();
            Mat grayImage = new Mat();
            Cv2.CvtColor(image, grayImage, ColorConversionCodes.BGR2GRAY);
            Cv2.GaussianBlur(grayImage, grayImage, new Size(5, 5), 0);
            Cv2.AdaptiveThreshold(grayImage, grayImage, 255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.BinaryInv, 11, 2);

            Mat[] contours;
            Mat hierarchy = new Mat();
            Cv2.FindContours(grayImage, out contours,   hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            foreach (var contour in contours)
            {
                Rect boundingBox = Cv2.BoundingRect(contour);
                if (boundingBox.Width > 10 && boundingBox.Height > 10) // Filtr minimalnego rozmiaru
                {
                    textRegions.Add(boundingBox);
                }
            }
            return textRegions;
        }
    }
}