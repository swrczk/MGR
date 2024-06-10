using System.Collections.Generic;
using Analysis.PropertiesProcessingStage.Model;
using OpenCvSharp;
using UnityEngine;

namespace Analysis.AccessibilityProcessingStage.Processor
{
    public class ColorAccessibilityProcessor
    { 

        private readonly float _contrastLowerThreshold = 4.5f;
        private readonly float _contrastUpperThreshold = 15.5f; 

 

        public List<ContrastAnalysisResultModel> SegmentsWithLowContrast(List<ContrastAnalysisResultModel> model)
        {
            return model.FindAll(m => m.Contrast < _contrastLowerThreshold);
        }

        private List<ContrastAnalysisResultModel> SegmentsWithHighContrast(List<ContrastAnalysisResultModel> model)
        {
            return model.FindAll(m => m.Contrast > _contrastUpperThreshold);
        }

        public List<(int, int)> DominantColorsWithLowContrast(DominantColorsPropertiesModel model)
        {
            var pairs = new List<(int, int)>();
            foreach (var (key, value) in model.ColorContrast)
            {
                if (value < _contrastLowerThreshold)
                    pairs.Add(key);
            }

            return pairs;
        }

        private List<(int, int)> DominantColorsWithHighContrast(DominantColorsPropertiesModel model)
        {
            var pairs = new List<(int, int)>();
            foreach (var (key, value) in model.ColorContrast)
            {
                if (value > _contrastUpperThreshold)
                    pairs.Add(key);
            }

            return pairs;
        }

        public float CalculateColorPercentage(Mat mat, Color color)
        {
            var colorScalar = new Scalar(color.r, color.g, color.b);

            var mask = new Mat(); // Create a mask where the specified color is white (255) and others are black (0)
            Cv2.InRange(mat, colorScalar, colorScalar, mask);

            var whitePixels = Cv2.CountNonZero(mask); // Calculate the number of white pixels in the mask
            var totalPixels = mat.Rows * mat.Cols;
            var colorPercentage = (whitePixels / (float)totalPixels);

            return colorPercentage;
        }
    }
}