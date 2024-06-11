using System.Text;
using UI;

namespace Analysis.ReportGenerationStage
{
    public class ReportGenerationStage
    {
        private readonly PDFGenerator _generator;
        private readonly DrawingProcessor _drawingProcessor;

        public ReportGenerationStage()
        {
            _generator = new PDFGenerator();
            _drawingProcessor = new DrawingProcessor();
        }

        public void Run(AnalysisModel model)
        {
            _generator.Create(model.ScreenData.screenName, model.ScreenData.fileName);
            DominantColorsReportPart(model);
            _generator.BreakPage();
            OcrReportPart(model);
            _generator.BreakPage();
            SegmentationReportPart(model);
            _generator.SaveCurrentPdf();
        }

        private void DominantColorsReportPart(AnalysisModel model)
        {
            _generator.AddSubTitle("Dominant Colors");
            var colorsMat = _drawingProcessor.CreateColorsMat(model.ScreenData.dominantColors);
            _generator.AddText("Dominujące kolory na ekranie to: ");
            _generator.AddImage(colorsMat.ToTexture());
            _generator.AddText(
                "Powyższy obrazek przedstawia dominujące kolory na ekranie. Za ich pomocą tekstura została posegmentowana według kolorów.");
            var colorSegmentation = model.PreprocessedModel.ColorSegmentedMat.ToTexture();
            _generator.AddImage(colorSegmentation);
        }

        private void OcrReportPart(AnalysisModel model)
        {
            _generator.AddSubTitle("Ocr");
            _generator.AddText("Korzystając z OCR odczytano następujące słowa: ");
            _generator.AddLineBreak();
            
            var ocrText = new StringBuilder();
            foreach (var word in model.PreprocessedModel.OcrResult.Text)
            {
                ocrText.Append(word);
                ocrText.Append(" ");
            }
            _generator.AddText(ocrText.ToString());
            _generator.AddLineBreak();
            
            _generator.AddText("Wykryte słowa zostały zaznaczone na obrazie:");
            
            var ocrResultHighlightedTexture = model.PreprocessedModel.OcrResult.HighlightedTexture;
            _generator.AddImage(ocrResultHighlightedTexture);
            
            _generator.AddText(
                "Model może być niedokładny. Jeżeli wynik wykrywania tekstu nie jest zadowalający (wykryte mniej niż 50% treści) należy powtórzyć analizę.");
        }
        
        private void SegmentationReportPart(AnalysisModel model)
        {
            _generator.AddSubTitle("Segmentacja");
            _generator.AddText("Obraz został podzielony na segmenty według kolorów, tak, aby każdy segment zawerał w przybliżeniu 2 kolory lub miał conajmniej rozmiary 10x10 pikseli.");

            var parameter = model.AnalysisParameters.GetParameter(FeatureType.SkipTextAnalysis);
            if( parameter is { isEnabled: false })
                _generator.AddText("Segmentacja została przeprowadzona z wyłączeniem obszarów zawierających tekst. Ponieważ obszary dzielone są za pomocą prostokątów może to prowadzić do sytuacji, w której znacząca ilość segmentów zostanie wykluczona. W takim przypadku zaleca się ponowne przeprowadzenie analizy z wyłączoną opcją pomijania obszarów zawierających tekst.");
            
            _generator.AddText("Poniżej przedstawiono wynik segmentacji:");
            var segmentationTexture = model.PreprocessedModel.SegmentedTextureModel.VisualizationMat.ToTexture();
            _generator.AddImage(segmentationTexture);
        }
    }
}