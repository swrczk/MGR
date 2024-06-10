using System.Collections.Generic;
using OpenCvSharp;

namespace Analysis.AccessibilityProcessingStage
{
    public class AccessibilityIndicatorModel
    {
        public float BaseValue { get; set; }
        public float Weight { get; set; }
        public List<Mat> Visualization { get; set; } = new List<Mat>();
    }
}