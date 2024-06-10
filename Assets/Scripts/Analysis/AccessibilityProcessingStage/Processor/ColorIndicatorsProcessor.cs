using System.Collections.Generic;
using Analysis.PropertiesProcessingStage.Model;
using OpenCvSharp;
using UI;
using UnityEngine;
using Rect = OpenCvSharp.Rect;

namespace Analysis.AccessibilityProcessingStage.Processor
{
    public class ColorIndicatorsProcessor
    {
        private ColorAccessibilityProcessor _colorAccessibilityProcessor;
        private readonly AnalysisParametersSO _parameters;

        public ColorIndicatorsProcessor(AnalysisParametersSO parameters,
            ColorAccessibilityProcessor colorAccessibilityProcessor)
        {
            _colorAccessibilityProcessor = colorAccessibilityProcessor;
            _parameters = parameters;
        }

        public ColorAccessibilityModel GenerateColorAccessibilityModel(PropertiesModel model, Mat originalMat)
        {
            var dominantColorsContrast =
                _colorAccessibilityProcessor.DominantColorsWithLowContrast(model.DominantColorsProperties);
            var segmentsWithLowContrast =
                _colorAccessibilityProcessor.SegmentsWithLowContrast(model.ContrastProperties);
            return new ColorAccessibilityModel()
            {
                // DominantColorsWithLowContrast =  
            };
        }

        private AccessibilityIndicatorModel CalculateSegmentsPropertyIndex(
            IEnumerable<ContrastAnalysisResultModel> models, Mat mat,
            Color color, FeatureType featureType)
        {
            // var imageArea = mat.Width * mat.Height;
            // var areaPercentage =  AreaPercentage(models, imageArea);
            // var visualization =  DrawAreas(models, mat, color);
            return new AccessibilityIndicatorModel()
            {
                // BaseValue = areaPercentage,
                // Weight = _parameters.GetWeight(featureType),
                // Visualization = visualization
            };
        }

        private float AreaPercentage(IEnumerable<SegmentModel> models, int imageArea)
        {
            var area = 0f;
            foreach (var model in models)
            {
                area += model.VolumePercentage;
            }

            return area / imageArea;
        }

        private Mat DrawAreas(IEnumerable<Rect> models, Mat mat, Color color)
        {
            var colorScalar = new Scalar(color.b * 255, color.g * 255, color.r * 255); // Convert Unity color to BGR

            var visualization = mat.Clone();
            foreach (var model in models)
            {
                var left = model.X;
                var top = model.Y;
                var right = model.X + model.Width;
                var bottom = model.Y + model.Height;

                if (right > left && bottom > top)
                {
                    Cv2.Rectangle(visualization, new Point(left, top), new Point(right, bottom), colorScalar, 2);
                }
            }

            return visualization;
        }

        public List<Mat> CreateColorPairMats(List<(Scalar, Scalar)> colorPairs)
        {
            var mats = new List<Mat>();

            foreach (var (color1, color2) in colorPairs)
            {
                Mat mat = new Mat(100, 200, MatType.CV_8UC3, new Scalar(0, 0, 0));
                Cv2.Rectangle(mat, new Rect(0, 0, 100, 100), color1, -1);
                Cv2.Rectangle(mat, new Rect(100, 0, 100, 100), color2, -1);
                mats.Add(mat);
            }

            return mats;
        }
    }
}