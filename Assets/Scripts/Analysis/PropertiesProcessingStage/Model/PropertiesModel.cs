using System.Collections.Generic;
using Analysis.PreprocessingStage.Model.TextModels;
using OpenCvSharp;

namespace Analysis.PropertiesProcessingStage.Model
{
    public class PropertiesModel
    {
        public TextDocumentModel DocumentModelProperties { get; set; }
        public List<ContrastAnalysisResultModel> ContrastProperties { get; set; }
        public Mat ContrastVisualizationMat { get; set; }
        public DominantColorsPropertiesModel DominantColorsProperties { get; set; }
    }
}