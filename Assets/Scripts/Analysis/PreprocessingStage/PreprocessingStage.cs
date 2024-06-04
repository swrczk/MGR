using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Analysis
{
    public class PreprocessingStage
    {
        private ColorProcessor _colorProcessor;
        private OcrProcessor _ocrProcessor;
        private SegmentationProcessor _segmentationProcessor;
        private int _clusterCount;
        private bool _excludeWords;

        public PreprocessingStage()
        {
            _colorProcessor = new ColorProcessor();
            _ocrProcessor = new OcrProcessor();
            _segmentationProcessor = new SegmentationProcessor();
        }

        public async UniTask<PreprocessedModel> Run(Texture2D texture)
        {
            var mat = AnalysisUtils.TextureToMat(texture);
            var grayMat = AnalysisUtils.MatToGrayScale(mat);
            var rect = new OpenCvSharp.Rect(0, 0, mat.Width, mat.Height);

            var dominantColors = _colorProcessor.GetDominantColors(mat, _clusterCount);
            var segmentedTexture = _colorProcessor.KMeansSegmentation(mat, _clusterCount);
            var (edgesMat, contours) = _colorProcessor.FindContours(grayMat);
            var contoursTexture = _colorProcessor.DrawContours(mat, contours);

            var ocrResult = await _ocrProcessor.RecognizeTextStructure(segmentedTexture);
            var documentStructure = _ocrProcessor.GetTextDocument(ocrResult);

            var fullSegmentation = _segmentationProcessor.FindSegments(grayMat, rect);
            var finalSegmentation = _excludeWords
                ? _segmentationProcessor.ExcludeRectsOverlappingWithWords(grayMat, fullSegmentation,
                    documentStructure.GetWords())
                : fullSegmentation;
            
            return new PreprocessedModel
            {
                DefaultMat = mat,
                GrayMat = grayMat,
                DominantColors = dominantColors,
                ColorSegmentedTexture = segmentedTexture,
                ContoursTexture = contoursTexture,
                EdgesMat = edgesMat,
                Contours = contours,
                OcrResult = ocrResult,
                DocumentStructure = documentStructure,
                SegmentedTexture = finalSegmentation
            };
        }
    }
}