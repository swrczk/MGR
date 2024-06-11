namespace Analysis.AccessibilityProcessingStage
{
    public class AccessibilityModel
    {
        public ColorAccessibilityModel ColorAccessibilityModel { get; set; }
        public TextAccessibilityModel TextAccessibilityModel { get; set; }
        public float OverallScore { get; set; }
        public float TextScore { get; set; }
        public float ColorScore { get; set; }
    }
}