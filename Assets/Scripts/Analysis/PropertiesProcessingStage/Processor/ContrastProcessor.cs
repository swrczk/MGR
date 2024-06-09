using System.Collections.Generic;
using Analysis.PropertiesProcessingStage.Model;
using OpenCvSharp;
using Rect = OpenCvSharp.Rect;

namespace Analysis.PropertiesProcessingStage.Processor
{
    public class ContrastProcessor
    {
        private readonly ColorPropertiesProcessor _colorPropertiesProcessor;
        
        private readonly float _contrastLowerThreshold;
        private readonly float _contrastUpperThreshold;

        private readonly Scalar _redScalar = new(0, 0, 255);
        private readonly Scalar _blueScalar = new(255, 0, 0);

        public ContrastProcessor(float contrastLowerThreshold, float contrastUpperThreshold, ColorPropertiesProcessor colorPropertiesProcessor )
        {
            _contrastLowerThreshold = contrastLowerThreshold;
            _contrastUpperThreshold = contrastUpperThreshold;
            _colorPropertiesProcessor = colorPropertiesProcessor;
        }

        public List<ContrastAnalysisResultModel> CheckContrast(List<SegmentModel> segments, Mat mat)
        {
            var analysisResults = new List<ContrastAnalysisResultModel>();
            foreach (var segment in segments)
            {
                var segmentResult = CheckContrast(mat, segment);
                analysisResults.Add(segmentResult);
            }

            return analysisResults;
        }

        public Mat GetContrastVisualizationTexture(List<ContrastAnalysisResultModel> analysisResults, Mat mat)
        {
            var outputImage = mat.Clone();
            var rectColor = new Scalar(0, 0, 0);
            foreach (var result in analysisResults)
            {
                if (result.ContrastResultType == ContrastResultType.Normal)
                    continue;

                rectColor = result.ContrastResultType switch
                {
                    ContrastResultType.Low => _redScalar,
                    ContrastResultType.High => _blueScalar,
                    _ => rectColor
                };

                DrawTransparentRectangle(result.SegmentModel.Rect, outputImage, rectColor);
            }

            return outputImage;
        }

        private ContrastAnalysisResultModel CheckContrast(Mat image, SegmentModel segmentModel)
        {
            var roi = new Mat(image, segmentModel.Rect);
            var overallContrast = CalculateOverallContrast(roi);

            if ((overallContrast < _contrastLowerThreshold))
            {
                return new ContrastAnalysisResultModel(segmentModel, ContrastResultType.Low, overallContrast);
            }

            if (overallContrast > _contrastUpperThreshold)
            {
                return new ContrastAnalysisResultModel(segmentModel, ContrastResultType.High, overallContrast);
            }

            return new ContrastAnalysisResultModel(segmentModel, ContrastResultType.Normal, overallContrast);
        }

        private void DrawTransparentRectangle(Rect rect, Mat image, Scalar color, float opacity = 0.5f)
        {
            var roi = new Mat(image, rect);
            var overlay = new Mat(roi.Size(), roi.Type(), color);
            Cv2.AddWeighted(overlay, opacity, roi, 1 - opacity, 0, roi);
        }


        //values from 1 to 21
        private float CalculateOverallContrast(Mat roi)
        {
            var gray = AnalysisUtils.MatToGrayScale(roi);

            // Znajdź najjaśniejszy i najciemniejszy piksel w wycinku
            double minVal, maxVal;
            Cv2.MinMaxLoc(gray, out minVal, out maxVal);

            // Konwersja najjaśniejszego i najciemniejszego piksela na Vec3b (zakładamy, że są to składowe RGB)
            var minColor = new Vec3b((byte)minVal, (byte)minVal, (byte)minVal);
            var maxColor = new Vec3b((byte)maxVal, (byte)maxVal, (byte)maxVal);

            return _colorPropertiesProcessor.CalculateContrastRatio(maxColor, minColor);
        }
    }
}