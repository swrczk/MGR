namespace Analysis.AccessibilityProcessingStage.Model
{
    public class TextAccessibilityModel
    {
        public AccessibilityIndicatorModel HighContrastTextColors { get; set; }
        public AccessibilityIndicatorModel LowContrastTextColors { get; set; }
        public AccessibilityIndicatorModel ImageAsTextBackground { get; set; }
        public AccessibilityIndicatorModel TooSmallFontSize { get; set; }
        public AccessibilityIndicatorModel TooSmallInterlineInTextBlock { get; set; }
        public AccessibilityIndicatorModel RightAlignedText { get; set; }
    }
}