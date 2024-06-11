using System.Collections.Generic;
using System.Linq;
using Analysis.PropertiesProcessingStage.Model;
using OpenCvSharp;
using UI;
using UnityEngine;
using Rect = OpenCvSharp.Rect;

namespace Analysis.AccessibilityProcessingStage.Processor
{
    public class ColorIndicatorsProcessor
    {
        private readonly ColorAccessibilityProcessor _colorAccessibilityProcessor;
        private readonly AnalysisParametersSO _parameters;

        public ColorIndicatorsProcessor(AnalysisParametersSO parameters,
            ColorAccessibilityProcessor colorAccessibilityProcessor)
        {
            _colorAccessibilityProcessor = colorAccessibilityProcessor;
            _parameters = parameters;
        }

        public ColorAccessibilityModel GenerateColorAccessibilityModel(PropertiesModel model, Mat colorSegmentedMat)
        {
            var dominantColorsLowContrast =
                _colorAccessibilityProcessor.DominantColorsWithLowContrast(model.DominantColorsProperties);
            var dominantColorsLowContrastIndex = CalculateColorsPropertyIndex(model.DominantColorsProperties,
                dominantColorsLowContrast, colorSegmentedMat, FeatureType.DominantColorsWithLowContrast);

            var dominantColorsHighContrast =
                _colorAccessibilityProcessor.DominantColorsWithHighContrast(model.DominantColorsProperties);
            var dominantColorsHighContrastIndex = CalculateColorsPropertyIndex(model.DominantColorsProperties,
                dominantColorsHighContrast, colorSegmentedMat, FeatureType.DominantColorsWithHighContrast);

            var segmentsWithLowContrast =
                _colorAccessibilityProcessor.SegmentsWithLowContrast(model.ContrastProperties);
            var segmentsWithLowContrastIndex = CalculateSegmentsPropertyIndex(segmentsWithLowContrast,
                FeatureType.SegmentsWithLowContrast);

            var segmentsWithHighContrast =
                _colorAccessibilityProcessor.SegmentsWithHighContrast(model.ContrastProperties);
            var segmentsWithHighContrastIndex = CalculateSegmentsPropertyIndex(segmentsWithHighContrast,
                FeatureType.SegmentsWithHighContrast);

            return new ColorAccessibilityModel()
            {
                DominantColorsWithLowContrast = dominantColorsLowContrastIndex,
                DominantColorsWithHighContrast = dominantColorsHighContrastIndex,
                SegmentsWithLowContrast = segmentsWithLowContrastIndex,
                SegmentsWithHighContrast = segmentsWithHighContrastIndex
            };
        }

        private AccessibilityIndicatorModel CalculateSegmentsPropertyIndex(
            IEnumerable<ContrastAnalysisResultModel> models, FeatureType featureType)
        {
            var segments = models.Select(x => x.SegmentModel).ToList();
            var areaPercentage = AreaPercentage(segments);

            Debug.LogError($"Segment area percentage: {areaPercentage} for {featureType} ");

            return new AccessibilityIndicatorModel()
            {
                BaseValue = areaPercentage,
                Weight = _parameters.GetWeight(featureType),
            };
        }

        private float AreaPercentage(IEnumerable<SegmentModel> models)
        {
            var area = 0f;
            foreach (var model in models)
            {
                area += model.VolumePercentage;
            }

            return area;
        }

        private List<AccessibilityIndicatorModel> CalculateColorsPropertyIndex(DominantColorsPropertiesModel allColors,
            List<(int, int)> models, Mat mat, FeatureType featureType)
        {
            var indicators = new List<AccessibilityIndicatorModel>();
            foreach (var (index1, index2) in models)
            {
                var color1 = allColors.DominantColors[index1];
                var color2 = allColors.DominantColors[index2];
                var colorPercentage1 = CalculateColorPercentage(mat, color1);
                var colorPercentage2 = CalculateColorPercentage(mat, color2);
                var colorPercentage = colorPercentage1 + colorPercentage2;
                var visualization = CreateColorPairMat(color1, color2);

                Debug.LogError($"Color percentage: {colorPercentage} for {featureType} ");

                indicators.Add(new AccessibilityIndicatorModel()
                {
                    BaseValue = colorPercentage,
                    Weight = _parameters.GetWeight(featureType),
                    Visualization = visualization
                });
            }

            return indicators;
        }


        private Mat CreateColorPairMat(Color color1, Color color2)
        {
            var scalar1 = new Scalar(color1.r, color1.g, color1.b);
            var scalar2 = new Scalar(color2.r, color2.g, color2.b);
            var mat = new Mat(100, 200, MatType.CV_8UC3, new Scalar(0, 0, 0));
            Cv2.Rectangle(mat, new Rect(0, 0, 100, 100), scalar1, -1);
            Cv2.Rectangle(mat, new Rect(100, 0, 100, 100), scalar2, -1);

            return mat;
        }

        private float CalculateColorPercentage(Mat mat, Color color)
        {
            var colorScalar = new Scalar(color.r, color.g, color.b);

            var mask = new Mat(); // Create a mask where the specified color is white (255) and others are black (0)
            Cv2.InRange(mat, colorScalar, colorScalar, mask);

            var whitePixels = Cv2.CountNonZero(mask); // Calculate the number of white pixels in the mask
            var totalPixels = mat.Rows * mat.Cols;
            var colorPercentage = (whitePixels / (float)totalPixels) * 100;

            return colorPercentage;
        }
    }
}