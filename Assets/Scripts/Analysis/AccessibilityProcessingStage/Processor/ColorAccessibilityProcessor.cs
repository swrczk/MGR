using System.Collections.Generic;
using Analysis.PropertiesProcessingStage.Model;

namespace Analysis.AccessibilityProcessingStage.Processor
{
    public class ColorAccessibilityProcessor
    { 

        private readonly float _contrastLowerThreshold = 4.5f;
        private readonly float _contrastUpperThreshold = 15.0f; 

 

        public List<ContrastAnalysisResultModel> SegmentsWithLowContrast(List<ContrastAnalysisResultModel> model)
        {
            return model.FindAll(m => m.Contrast < _contrastLowerThreshold);
        }

        public List<ContrastAnalysisResultModel> SegmentsWithHighContrast(List<ContrastAnalysisResultModel> model)
        {
            return model.FindAll(m => m.Contrast > _contrastUpperThreshold);
        }

        public List<(int, int)> DominantColorsWithLowContrast(DominantColorsPropertiesModel model)
        {
            var pairs = new List<(int, int)>();
            foreach (var (pair, contrastRatio) in model.ColorContrast)
            {
                if (contrastRatio < _contrastLowerThreshold)
                    pairs.Add(pair);
            }

            return pairs;
        }

        public List<(int, int)> DominantColorsWithHighContrast(DominantColorsPropertiesModel model)
        {
            var pairs = new List<(int, int)>();
            foreach (var (pair, contrastRatio) in model.ColorContrast)
            {
                if (contrastRatio > _contrastUpperThreshold)
                    pairs.Add(pair);
            }

            return pairs;
        }
    }
}