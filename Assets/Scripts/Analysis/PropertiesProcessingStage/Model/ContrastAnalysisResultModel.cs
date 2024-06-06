namespace Analysis
{
    public enum ContrastResultType
    {
        Low,
        High,
        Normal
    }

    public class ContrastAnalysisResultModel
    {
        public SegmentModel SegmentModel { get; private set; }
        public float Contrast { get; private set; }
        public ContrastResultType ContrastResultType { get; private set; }

        public ContrastAnalysisResultModel(SegmentModel segmentModel, ContrastResultType low, float overallContrast)
        {
            SegmentModel = segmentModel;
            ContrastResultType = low;
            Contrast = overallContrast;
        }
    }
}