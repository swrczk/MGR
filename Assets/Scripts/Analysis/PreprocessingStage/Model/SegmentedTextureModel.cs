using System.Collections.Generic;
using OpenCvSharp;

namespace Analysis.PreprocessingStage.Model
{
    public class SegmentedTextureModel
    {
        public Rect ImageRect { get; set; }
        public List<SegmentModel> Segments { get; set; }
        public Mat VisualizationMat { get; set; }
        
        public SegmentedTextureModel(Rect imageRect, List<SegmentModel> segments, Mat visualizationMat)
        {
            ImageRect = imageRect;
            Segments = segments;
            VisualizationMat = visualizationMat;
            RecalculateVolumePercentage();
        }

        public void RecalculateVolumePercentage()
        {
            float imageArea = ImageRect.Width * ImageRect.Height;

            foreach (var segment in Segments)
            {
                float rectArea = segment.Rect.Width * segment.Rect.Height;
                float volumePercentage = (rectArea / imageArea) * 100;
                segment.VolumePercentage = volumePercentage;
            }
        }
    }
}