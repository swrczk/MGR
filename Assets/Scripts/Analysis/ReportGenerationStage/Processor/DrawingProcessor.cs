using System.Collections.Generic;
using OpenCvSharp;
using UnityEngine;
using Rect = OpenCvSharp.Rect;

namespace Analysis.ReportGenerationStage
{
    public class DrawingProcessor
    {
        public Mat CreateColorsMat(List<Color> colors)
        {
            var mat = new Mat(100, 100 * colors.Count, MatType.CV_8UC3, new Scalar(0, 0, 0));
            for (var i = 0; i < colors.Count; i++)
            {
                var color = colors[i];
                var scalar = AnalysisUtils.ColorToScalar(color);
                Cv2.Rectangle(mat, new Rect(i * 100, 0, 100, 100), scalar, -1);
            }

            return mat;
        }
    }
}