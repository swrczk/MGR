using Analysis.PreprocessingStage.Model;
using Analysis.PreprocessingStage.Processor;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Analysis.PreprocessingStage
{
    public class PreprocessingStage
    {
        private readonly ColorProcessor _colorProcessor;
        private readonly OcrProcessor _ocrProcessor;
        private readonly SegmentationProcessor _segmentationProcessor;

        private readonly int _clusterCount;
        private readonly bool _excludeWords;

        public PreprocessingStage(ImageProcessingParametersSo parameters)
        {
            _clusterCount = parameters.clusterCount;
            _excludeWords = parameters.excludeWords;
            _colorProcessor = new ColorProcessor(parameters.defaultContoursColor);
            _ocrProcessor = new OcrProcessor();
            _segmentationProcessor = new SegmentationProcessor(parameters.targetColorAmount, parameters.colorThreshold,
                parameters.entropyThreshold);
        }

        public async UniTask<PreprocessedModel> Run(Texture2D texture)
        {
            var mat = AnalysisUtils.TextureToMat(texture);
            var grayMat = AnalysisUtils.MatToGrayScale(mat);
            var grayMatWithColorChannels = AnalysisUtils.AddColorChannelsToGrayScale(grayMat);

            var dominantColors = _colorProcessor.GetDominantColors(mat, _clusterCount);
            var colorSegmentedMat = _colorProcessor.KMeansSegmentation(mat, _clusterCount);
            var (edgesMat, contours) = _colorProcessor.FindContours(grayMat);
            var contoursTexture = _colorProcessor.DrawContours(mat, contours);

            var ocrResult = await _ocrProcessor.RecognizeTextStructure(AnalysisUtils.MatToTexture(colorSegmentedMat));
            var documentStructure = _ocrProcessor.GetTextDocument(ocrResult);

            var fullSegmentation = _segmentationProcessor.FindSegments(mat);
            var finalSegmentation = _excludeWords
                ? _segmentationProcessor.ExcludeRectsOverlappingWithWords(grayMatWithColorChannels, fullSegmentation,
                    documentStructure.GetWords())
                : fullSegmentation;

            return new PreprocessedModel
            {
                DefaultMat = mat,
                GrayMat = grayMat,
                GrayMatWithColorChannels = grayMatWithColorChannels,
                DominantColors = dominantColors,
                ColorSegmentedMat = colorSegmentedMat,
                ContoursTexture = contoursTexture,
                EdgesMat = edgesMat,
                Contours = contours,
                OcrResult = ocrResult,
                DocumentModelStructure = documentStructure,
                SegmentedTextureModel = finalSegmentation
            };
        }
    }
}