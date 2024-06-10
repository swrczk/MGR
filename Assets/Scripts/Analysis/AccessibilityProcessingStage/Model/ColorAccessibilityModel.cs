namespace Analysis.AccessibilityProcessingStage
{
    public class ColorAccessibilityModel
    {
        public AccessibilityIndicatorModel DominantColorsWithLowContrast { get; set; }
        public AccessibilityIndicatorModel DominantColorsWithHighContrast { get; set; }
        public AccessibilityIndicatorModel SegmentsWithLowContrast { get; set; }
        public AccessibilityIndicatorModel SegmentsWithHighContrast { get; set; }
    }
}