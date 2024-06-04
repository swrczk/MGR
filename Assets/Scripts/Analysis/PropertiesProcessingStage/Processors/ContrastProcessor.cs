using System.Collections.Generic;
using OpenCvSharp;
using Rect = OpenCvSharp.Rect;

namespace Analysis
{
    public class ContrastProcessor
    {
        //move to config
        private readonly float _contrastLowerThreshold = 4.5f;
        private readonly float _contrastUpperThreshold = 15.0f;

        private readonly Scalar _redScalar = new(0, 0, 255);
        private readonly Scalar _blueScalar = new(255, 0, 0);
        
        private ColorPropertiesProcessor _colorPropertiesProcessor = new();

        public List<ContrastAnalysisResult>  CheckContrast(PreprocessedModel model)
        {
            var analysisResults = new List<ContrastAnalysisResult>();
            foreach (var segment in model.SegmentedTexture.Segments)
            {
                var segmentResult = CheckContrast(model.DefaultMat, segment);
                analysisResults.Add(segmentResult);
            }

            return analysisResults;
        }
        
        public Mat GetContrastVisualizationTexture(List<ContrastAnalysisResult> analysisResults, Mat mat)
        {
            var outputImage = AnalysisUtils.AddColorChannelsToGrayScale(mat);
            var rectColor = new Scalar(0, 0, 0);
            foreach (var result in analysisResults)
            {
                if(result.ContrastResultType == ContrastResultType.Normal)
                    continue;
                
                rectColor = result.ContrastResultType switch
                {
                    ContrastResultType.Low => _redScalar,
                    ContrastResultType.High => _blueScalar,
                    _ => rectColor
                };

                DrawTransparentRectangle(result.Segment.Rect, outputImage, rectColor);
            }

            return outputImage;
        }

        private ContrastAnalysisResult CheckContrast(Mat image, Segment segment)
        {
            var roi = new Mat(image, segment.Rect);
            var overallContrast = CalculateOverallContrast(roi);

            if ((overallContrast < _contrastLowerThreshold))
            {
                return new ContrastAnalysisResult(segment, ContrastResultType.Low, overallContrast);
            }

            if (overallContrast > _contrastUpperThreshold)
            {
                return new ContrastAnalysisResult(segment, ContrastResultType.High, overallContrast);
            }

            return new ContrastAnalysisResult(segment, ContrastResultType.Normal, overallContrast);
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