using OpenCvSharp;

namespace Analysis.PreprocessingStage.Model
{
    public class SegmentModel
    {
        public Rect Rect { get; set; }
        public float VolumePercentage { get; set; }
    }
}