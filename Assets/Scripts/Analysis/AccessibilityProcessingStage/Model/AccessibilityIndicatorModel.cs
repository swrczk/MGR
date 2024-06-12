using OpenCvSharp;

namespace Analysis.AccessibilityProcessingStage
{
    public class AccessibilityIndicatorModel
    {
        public float BaseValue { get; set; } // percentage area of the screen
        public float Weight { get; set; }
        public Mat Visualization { get; set; }
    }
    
    public class DominantColorAccessibilityIndicatorModel:AccessibilityIndicatorModel
    {
        public float Contrast { get; set; }
    }
}