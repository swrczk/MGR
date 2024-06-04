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
        private int _clusterCount = 5;
        private bool _excludeWords = true;

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

            var dominantColors = _colorProcessor.GetDominantColors(mat, _clusterCount);
            var colorSegmentedMat = _colorProcessor.KMeansSegmentation(mat, _clusterCount);
            var (edgesMat, contours) = _colorProcessor.FindContours(grayMat);
            var contoursTexture = _colorProcessor.DrawContours(mat, contours);

            var ocrResult = await _ocrProcessor.RecognizeTextStructure(AnalysisUtils.MatToTexture(colorSegmentedMat));
            var documentStructure = _ocrProcessor.GetTextDocument(ocrResult);

            var fullSegmentation = _segmentationProcessor.FindSegments(mat);
            var finalSegmentation = _excludeWords
                ? _segmentationProcessor.ExcludeRectsOverlappingWithWords(grayMat, fullSegmentation,
                    documentStructure.GetWords())
                : fullSegmentation;
            
            return new PreprocessedModel
            {
                DefaultMat = mat,
                GrayMat = grayMat,
                DominantColors = dominantColors,
                ColorSegmentedMat = colorSegmentedMat,
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