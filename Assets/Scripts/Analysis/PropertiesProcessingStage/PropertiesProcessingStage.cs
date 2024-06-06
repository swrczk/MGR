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

        public PropertiesProcessingStage()
        {
            _wordsColorProcessor = new WordsColorProcessor();
            _contrastProcessor = new ContrastProcessor();
            _wordPropertiesProcessor = new WordPropertiesProcessor();
            _linePropertiesProcessor = new LinePropertiesProcessor();
            _textBlockPropertiesProcessor = new TextBlockPropertiesProcessor();
        }

        public PropertiesModel Run(PreprocessedModel model, Texture2D originalTexture)
        {
            var documentProperties = DefineDocumentProperties(model);
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

        private TextDocumentModel DefineDocumentProperties(PreprocessedModel model)
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
    }
}