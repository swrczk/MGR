using System.Collections.Generic;

namespace Analysis.AccessibilityProcessingStage.Model
{
    public class ColorAccessibilityModel
    {
        public List<DominantColorAccessibilityIndicatorModel> DominantColorsWithLowContrast { get; set; }
        public List<DominantColorAccessibilityIndicatorModel> DominantColorsWithHighContrast { get; set; }
        public AccessibilityIndicatorModel SegmentsWithLowContrast { get; set; }
        public AccessibilityIndicatorModel SegmentsWithHighContrast { get; set; }
        public AccessibilityIndicatorModel OverallColorLowContrast { get; set; }
        public AccessibilityIndicatorModel OverallColorContrastHigh { get; set; }
    }
}