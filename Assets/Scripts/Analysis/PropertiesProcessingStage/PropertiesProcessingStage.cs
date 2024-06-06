using UnityEngine;

namespace Analysis.PropertiesProcessingStage
{
    public class PropertiesProcessingStage
    {
        private DocumentProcessor _documentProcessor;
        private ContrastProcessor _contrastProcessor;

        public PropertiesProcessingStage()
        {
            _documentProcessor = new DocumentProcessor();
            _contrastProcessor = new ContrastProcessor();
        }

        public PropertiesModel Run(PreprocessedModel model, Texture2D originalTexture)
        {
            var documentProperties = DefineDocumentProperties(model);
            var contrastProperties = _contrastProcessor.CheckContrast(model.SegmentedTexture.Segments,
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
                var colors = _documentProcessor.ColorsSortedByFrequency(model.ColorSegmentedMat, word.Box);
                var isUniformBackground = _documentProcessor.IsBackgroundUniform(model.DefaultMat, word);

                var (textColor, backgroundColor) = _documentProcessor.DefineWordColors(colors, word.Text.Length < 2);

                word.SaveTextColor(backgroundColor);
                word.SaveBackgroundColor(textColor, isUniformBackground);
            }

            return result;
        }
    }
}