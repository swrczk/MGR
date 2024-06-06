using System.Collections.Generic;
using OpenCvSharp;

namespace Analysis.PropertiesProcessingStage
{
    public class PropertiesModel
    {
        public TextDocumentModel DocumentModelProperties { get; set; }
        public List<ContrastAnalysisResultModel> ContrastProperties { get; set; }
        public Mat ContrastVisualizationMat { get; set; }
    }
}