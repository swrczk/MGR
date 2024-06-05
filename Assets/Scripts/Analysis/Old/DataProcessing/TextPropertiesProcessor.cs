using System.Collections.Generic;
using System.Linq;
using OpenCvSharp;
using UnityEngine;

namespace Analysis
{
    //done
    public class TextPropertiesProcessor : MonoBehaviour
    {
        private const float EntropyThreshold = 5.0f;
        private const int EdgesPerCharThreshold = 10;

        public void DetectTextColor(ScreenForAnalysisSo screenForAnalysisData)
        {
            var texture = screenForAnalysisData.ProcessedImages[ImageProcessingType.ColorSegmentedTexture];
            var image = AnalysisUtils.TextureToMat(texture);
            var originalImage = AnalysisUtils.TextureToMat(screenForAnalysisData.texture);
            var wordBoxes = screenForAnalysisData.textDocument.GetWords();
            foreach (var word in wordBoxes)
            {
                var colors = ColorsSortedByFrequency(image, word.Box);
                var isUniformBackground = IsUniformBackground(originalImage, word);

                //for single letter text color is dominant color, for longer text background color is dominant
                Color textColor;
                Color backgroundColor;
                if (word.Text.Length == 1)
                {
                    textColor = AnalysisUtils.Vec3ToColor(colors[0]);
                    backgroundColor = colors.Count < 2 //ex.: letter "I" takes whole box
                        ? new Color(255f - textColor.r, 255f - textColor.g, 255f - textColor.b)
                        : AnalysisUtils.Vec3ToColor(colors[1]);
                }
                else
                {
                    textColor = AnalysisUtils.Vec3ToColor(colors[1]);
                    backgroundColor = AnalysisUtils.Vec3ToColor(colors[0]);
                }

                word.SaveTextColor(backgroundColor);
                word.SaveBackgroundColor(textColor, isUniformBackground);

                Debug.Log(
                    $"[TextPropertiesProcessor] GetDominantColor ({word.Text}): \n TextColor: {word.TextColor} - TextBackgroundColor: {word.TextBackgroundColor} - IsUniformBackground: {word.IsUniformBackground}");
            }
        }

        private List<Vec3b> ColorsSortedByFrequency(Mat image, Box box)
        {
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


        private bool IsUniformBackground(Mat image, Word word)
        {
            var box = word.Box;
            var charCount = word.Text.Length;
            var roi = new Mat(image, new OpenCvSharp.Rect(box.x, box.y, box.w, box.h));
            var gray = AnalysisUtils.MatToGrayScale(roi);

            var edgesCondition = CheckUniformBackgroundEdgeCondition(gray, charCount);
            var entropyCondition = CheckUniformBackgroundEntropyCondition(gray, charCount);

            return edgesCondition && entropyCondition;
        }

        private static bool CheckUniformBackgroundEntropyCondition(Mat gray, int charCount)
        {
            var entropy = AnalysisUtils.GetHistogramEntropy(gray);
            
            // Adjust the entropy threshold based on the length of the text
            var adjustedEntropyThreshold = EntropyThreshold + 0.15 * charCount;
            var entropyCondition = entropy < adjustedEntropyThreshold;
            return entropyCondition;
        }

        private bool CheckUniformBackgroundEdgeCondition(Mat gray, int charCount)
        {
            var edgesCount = GetNumberOfEdges(gray);
            var edgesThreshold = EdgesPerCharThreshold * charCount;
            var edgesCondition = edgesCount < edgesThreshold;
            return edgesCondition;
        }

        private int GetNumberOfEdges(Mat gray)
        {
            var edges = new Mat();
            Cv2.Canny(gray, edges, 50, 100);
            Cv2.FindContours(edges, out var contours, out var hierarchy, RetrievalModes.Tree,
                ContourApproximationModes.ApproxSimple);

            return contours.Length;
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
    }
}