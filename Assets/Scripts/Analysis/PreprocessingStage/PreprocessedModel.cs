using System.Collections.Generic;
using OpenCvSharp;
using UnityEngine;

namespace Analysis
{
    public class PreprocessedModel
    {
        public Mat DefaultMat { get; set; }
        public Mat GrayMat { get; set; }
        public Mat EdgesMat { get; set; }
        public Point[][] Contours { get; set; }
        public List<Color> DominantColors { get; set; }
        public Mat ColorSegmentedMat { get; set; }
        public Texture2D ContoursTexture { get; set; }
        public OcrResult OcrResult { get; set; }
        public TextDocument DocumentStructure { get; set; }
        public SegmentedTexture SegmentedTexture { get; set; }
    }
}