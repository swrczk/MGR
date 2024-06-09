using OpenCvSharp;

namespace Analysis.AccessibilityProcessingStage
{
    public class AccessibilityIndicatorModel
    {
        public float BaseValue { get; set; }
        public float Weight { get; set; }
        public Mat Visualization { get; set; }
    }
}