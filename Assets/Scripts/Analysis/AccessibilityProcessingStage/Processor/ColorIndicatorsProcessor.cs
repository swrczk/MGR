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
            var overallColorLowContrast =
                OverallColorContrast(dominantColorsLowContrastIndex, FeatureType.DominantColorsWithLowContrast);

            var dominantColorsHighContrast =
                _colorAccessibilityProcessor.DominantColorsWithHighContrast(model.DominantColorsProperties);
            var dominantColorsHighContrastIndex = CalculateColorsPropertyIndex(model.DominantColorsProperties,
                dominantColorsHighContrast, colorSegmentedMat, FeatureType.DominantColorsWithHighContrast);
            var overallColorContrastHigh = OverallColorContrast(dominantColorsHighContrastIndex,
                FeatureType.DominantColorsWithHighContrast);

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
                OverallColorLowContrast = overallColorLowContrast,
                DominantColorsWithHighContrast = dominantColorsHighContrastIndex,
                OverallColorContrastHigh = overallColorContrastHigh,
                SegmentsWithLowContrast = segmentsWithLowContrastIndex,
                SegmentsWithHighContrast = segmentsWithHighContrastIndex
            };
        }

        private AccessibilityIndicatorModel OverallColorContrast(List<DominantColorAccessibilityIndicatorModel> model,
            FeatureType featureType)
        {
            var weight = _parameters.GetWeight(featureType);
            var value = model.Sum(x => x.BaseValue * x.Weight);

            return new AccessibilityIndicatorModel()
            {
                BaseValue = value,
                Weight = weight
            };
        }

        private AccessibilityIndicatorModel CalculateSegmentsPropertyIndex(
            IEnumerable<ContrastAnalysisResultModel> models, FeatureType featureType)
        {
            var segments = models.Select(x => x.SegmentModel).ToList();
            var areaPercentage = AreaPercentage(segments);

            Debug.Log($"Segment area percentage: {areaPercentage} for {featureType} ");

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

        private List<DominantColorAccessibilityIndicatorModel> CalculateColorsPropertyIndex(
            DominantColorsPropertiesModel allColors,
            List<(int, int)> models, Mat mat, FeatureType featureType)
        {
            var indicators = new List<DominantColorAccessibilityIndicatorModel>();
            foreach (var (index1, index2) in models)
            {
                var color1 = allColors.DominantColors[index1];
                var color2 = allColors.DominantColors[index2];
                var colorPercentage1 = CalculateColorPercentage(mat, color1);
                var colorPercentage2 = CalculateColorPercentage(mat, color2);
                var colorPercentage = colorPercentage1 + colorPercentage2;
                var visualization = CreateColorPairMat(color1, color2);
                var contrast = allColors.ColorContrast[(index1, index2)];
                Debug.Log($"Color percentage: {colorPercentage} for {featureType} ");

                indicators.Add(new DominantColorAccessibilityIndicatorModel()
                {
                    BaseValue = colorPercentage,
                    Weight = _parameters.GetWeight(featureType),
                    Visualization = visualization,
                    Contrast = contrast
                });
            }

            return indicators;
        }


        private Mat CreateColorPairMat(Color color1, Color color2)
        {
            var scalar1 = AnalysisUtils.ColorToScalar(color1);
            var scalar2 = AnalysisUtils.ColorToScalar(color2);
            var mat = new Mat(100, 200, MatType.CV_8UC3, new Scalar(0, 0, 0));
            Cv2.Rectangle(mat, new Rect(0, 0, 100, 100), scalar1, -1);
            Cv2.Rectangle(mat, new Rect(100, 0, 100, 100), scalar2, -1);

            return mat;
        }

        private float CalculateColorPercentage(Mat mat, Color color, int tolerance = 10)
        {
            var lowerBound = new Scalar(
                Mathf.Clamp((float)(color.b * 255) - tolerance, 0, 255),
                Mathf.Clamp((float)(color.g * 255) - tolerance, 0, 255),
                Mathf.Clamp((float)(color.r * 255) - tolerance, 0, 255)
            );

            var upperBound = new Scalar(
                Mathf.Clamp((float)(color.b * 255) + tolerance, 0, 255),
                Mathf.Clamp((float)(color.g * 255) + tolerance, 0, 255),
                Mathf.Clamp((float)(color.r * 255) + tolerance, 0, 255)
            );

            var mask = new Mat();
            Cv2.InRange(mat, lowerBound, upperBound, mask);

            var whitePixels = Cv2.CountNonZero(mask); // Calculate the number of white pixels in the mask
            var totalPixels = mat.Rows * mat.Cols;
            var colorPercentage = (whitePixels / (float)totalPixels) * 100;

            return colorPercentage;
        }
    }
}