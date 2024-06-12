using Analysis.PreprocessingStage.Model;
using Analysis.PreprocessingStage.Model.TextModels;
using Analysis.PropertiesProcessingStage.Model;
using Analysis.PropertiesProcessingStage.Processor;
using OpenCvSharp;
using UnityEngine;

namespace Analysis.PropertiesProcessingStage
{
    public class PropertiesProcessingStage
    {
        private readonly WordsColorProcessor _wordsColorProcessor;
        private readonly ContrastProcessor _contrastProcessor;
        private readonly WordPropertiesProcessor _wordPropertiesProcessor;
        private readonly LinePropertiesProcessor _linePropertiesProcessor;
        private readonly TextBlockPropertiesProcessor _textBlockPropertiesProcessor;
        private readonly TextDocumentPropertiesProcessor _textDocumentPropertiesProcessor;
        private readonly DominantColorsPropertiesProcessor _dominantColorsPropertiesProcessor;
        private readonly ColorPropertiesProcessor _colorPropertiesProcessor;

        public PropertiesProcessingStage(ImageProcessingParametersSo parameters)
        {
            _colorPropertiesProcessor = new ColorPropertiesProcessor();
            _wordsColorProcessor =
                new WordsColorProcessor(parameters.entropyThresholdWords, parameters.edgesPerCharThreshold);
            _contrastProcessor =
                new ContrastProcessor(parameters.contrastLowerThreshold, parameters.contrastUpperThreshold, _colorPropertiesProcessor);
            _wordPropertiesProcessor = new WordPropertiesProcessor();
            _linePropertiesProcessor = new LinePropertiesProcessor();
            _textBlockPropertiesProcessor = new TextBlockPropertiesProcessor();
            _textDocumentPropertiesProcessor = new TextDocumentPropertiesProcessor();
            _dominantColorsPropertiesProcessor = new DominantColorsPropertiesProcessor(_colorPropertiesProcessor);
        }

        public PropertiesModel Run(PreprocessedModel model, Texture2D originalTexture)
        {
            var document = model.DocumentModelStructure;
            DefineDocumentStructureProperties(document, model.DefaultMat);
            DefineDocumentColorProperties(document, model.DefaultMat, model.ColorSegmentedMat);
            var contrastProperties = _contrastProcessor.CheckContrast(model.SegmentedTextureModel.Segments,
                AnalysisUtils.TextureToMat(originalTexture));
            var visualizationMat =
                _contrastProcessor.GetContrastVisualizationTexture(contrastProperties, model.GrayMatWithColorChannels);
            var dominantColorsProperties = new DominantColorsPropertiesModel
            {
                DominantColors = model.DominantColors,
                ColorContrast = _dominantColorsPropertiesProcessor.Process(model.DominantColors)
            };
            return new PropertiesModel
            {
                DocumentModelProperties = document,
                ContrastProperties = contrastProperties,
                ContrastVisualizationMat = visualizationMat,
                DominantColorsProperties = dominantColorsProperties
            };
        }

        private void DefineDocumentColorProperties(TextDocumentModel document, Mat defaultMat, Mat colorSegmentedMat)
        {
            foreach (var word in document.GetWords())
            {
                var colors = _wordsColorProcessor.ColorsSortedByFrequency(colorSegmentedMat, word.Box);
                var isUniformBackground = _wordsColorProcessor.IsBackgroundUniform(defaultMat, word);

                var (textColor, backgroundColor) = _wordsColorProcessor.DefineWordColors(colors, word.Text.Length < 2);

                word.TextBackgroundColor = backgroundColor;
                word.TextColor = textColor;
                word.IsUniformBackground = isUniformBackground;
            }
        }

        private void DefineDocumentStructureProperties(TextDocumentModel document, Mat defaultMat)
        {
            var imageArea = defaultMat.Width * defaultMat.Height;

            foreach (var word in document.GetWords())
            {
                _wordPropertiesProcessor.ProcessWordProperties(word, imageArea);
            }

            foreach (var line in document.GetLines())
            {
                _linePropertiesProcessor.ProcessLineProperties(line, imageArea);
            }

            foreach (var block in document.TextBlocks)
            {
                _textBlockPropertiesProcessor.ProcessTextBlockProperties(block, imageArea);
            }

            _textDocumentPropertiesProcessor.ProcessTextDocumentProperties(document, defaultMat);
        }
    }
}