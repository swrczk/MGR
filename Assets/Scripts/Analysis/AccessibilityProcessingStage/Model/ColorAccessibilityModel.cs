using System.Collections.Generic;

namespace Analysis.AccessibilityProcessingStage
{
    public class ColorAccessibilityModel
    {
        public List<AccessibilityIndicatorModel> DominantColorsWithLowContrast { get; set; }
        public List<AccessibilityIndicatorModel> DominantColorsWithHighContrast { get; set; }
        public AccessibilityIndicatorModel SegmentsWithLowContrast { get; set; }
        public AccessibilityIndicatorModel SegmentsWithHighContrast { get; set; }
    }
}