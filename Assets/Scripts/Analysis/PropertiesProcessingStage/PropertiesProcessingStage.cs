using Analysis.PropertiesProcessingStage.Model;
using Analysis.PropertiesProcessingStage.Processor;
using UnityEngine;

namespace Analysis.PropertiesProcessingStage
{
    public class PropertiesProcessingStage
    {
        private WordsColorProcessor _wordsColorProcessor;
        private ContrastProcessor _contrastProcessor;
        private WordPropertiesProcessor _wordPropertiesProcessor;
        private LinePropertiesProcessor _linePropertiesProcessor;
        private TextBlockPropertiesProcessor _textBlockPropertiesProcessor;
        private TextDocumentPropertiesProcessor _textDocumentPropertiesProcessor;

        public PropertiesProcessingStage()
        {
            _wordsColorProcessor = new WordsColorProcessor();
            _contrastProcessor = new ContrastProcessor();
            _wordPropertiesProcessor = new WordPropertiesProcessor();
            _linePropertiesProcessor = new LinePropertiesProcessor();
            _textBlockPropertiesProcessor = new TextBlockPropertiesProcessor();
            _textDocumentPropertiesProcessor = new TextDocumentPropertiesProcessor();
        }

        public PropertiesModel Run(PreprocessedModel model, Texture2D originalTexture)
        {
            DefineDocumentStructureProperties(model);
            var documentProperties = DefineDocumentColorProperties(model);
            var contrastProperties = _contrastProcessor.CheckContrast(model.SegmentedTextureModel.Segments,
                AnalysisUtils.TextureToMat(originalTexture));
            var visualizationMat =
                _contrastProcessor.GetContrastVisualizationTexture(contrastProperties, model.GrayMatWithColorChannels);
            return new PropertiesModel
            {
                DocumentModelProperties = documentProperties,
                ContrastProperties = contrastProperties,
                ContrastVisualizationMat = visualizationMat
            };
        }

        private TextDocumentModel DefineDocumentColorProperties(PreprocessedModel model)
        {
            var result = model.DocumentModelStructure;
            foreach (var word in result.GetWords())
            {
                var colors = _wordsColorProcessor.ColorsSortedByFrequency(model.ColorSegmentedMat, word.Box);
                var isUniformBackground = _wordsColorProcessor.IsBackgroundUniform(model.DefaultMat, word);

                var (textColor, backgroundColor) = _wordsColorProcessor.DefineWordColors(colors, word.Text.Length < 2);

                word.TextBackgroundColor = backgroundColor;
                word.TextColor = textColor;
                word.IsUniformBackground = isUniformBackground;
            }

            return result;
        }

        private TextDocumentModel DefineDocumentStructureProperties(PreprocessedModel model)
        {
            var mat = model.DefaultMat;
            var imageArea = mat.Width * mat.Height;
            var document = model.DocumentModelStructure;
            foreach (var word in document.GetWords())
            {
                _wordPropertiesProcessor.ProcessWordProperties(  word, imageArea);
            }

            foreach (var line in document.GetLines())
            {
                _linePropertiesProcessor.ProcessLineProperties(  line, imageArea);
            }

            foreach (var block in document.TextBlocks)
            {
                _textBlockPropertiesProcessor.ProcessTextBlockProperties(  block, imageArea);
            }
            
            _textDocumentPropertiesProcessor.ProcessTextDocumentProperties(document);

            return document;
        }
    }
}