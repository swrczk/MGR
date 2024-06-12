using System.Text;
using Analysis.ReportGenerationStage.Processor;
using UI;

namespace Analysis.ReportGenerationStage
{
    public class ReportGenerationStage
    {
        private readonly PdfGenerator _generator;
        private readonly DrawingProcessor _drawingProcessor;

        public ReportGenerationStage()
        {
            _generator = new PdfGenerator();
            _drawingProcessor = new DrawingProcessor();
        }

        public void Run(AnalysisModel model)
        {
            _generator.Create(model.ScreenData.screenName, model.ScreenData.fileName);

            FirstPageReportPart(model);
            _generator.BreakPage();

            _generator.AddTitle("Wartości Skali Dostępności");
            AccessibilityScaleReportPart(model);
            _generator.BreakPage();

            _generator.AddTitle("Szczegóły uzyskanego wyniku");
            DominantColorsReportPart(model);
            DominantColorsContrastReportPart(model);
            _generator.BreakPage();

            _generator.AddTitle("Etapy Przetwarzania Obrazu");
            OcrReportPart(model);
            _generator.BreakPage();

            SegmentationReportPart(model);
            
            _generator.SaveCurrentPdf();
        }

        private void FirstPageReportPart(AnalysisModel model)
        {
            _generator.AddTitle("Raport Analizy Dostępności Interfejsu");
            _generator.AddSubTitle($"Dla zrzutu ekranu: {model.ScreenData.screenName}");
            _generator.AddText("Analiza została przeprowadzona na podstawie pliku: ");
            _generator.AddImage(model.ScreenData.texture);
        }

        private void AccessibilityScaleReportPart(AnalysisModel model)
        {
            _generator.AddSubTitle($"Wynik ogólny: {model.AccessibilityModel.OverallScore}");
            _generator.AddLineBreak();

            _generator.AddText($"Wynik skali dostępności dla parametrów tekstu: {model.AccessibilityModel.TextScore}");
            _generator.AddText(
                $"Wynik skali dostępności dla parametrów kolorów: {model.AccessibilityModel.ColorScore}");
            _generator.AddLineBreak();

            _generator.AddSubTitle($"Składowe wyniku");
            _generator.AddAccessibilityIndicatorsResultTable(model.AccessibilityModel);
        }

        private void DominantColorsContrastReportPart(AnalysisModel model)
        {
            if (model.AccessibilityModel.ColorAccessibilityModel.DominantColorsWithLowContrast.Count == 0
                && model.AccessibilityModel.ColorAccessibilityModel.DominantColorsWithHighContrast.Count == 0)
                return;
            _generator.AddSubTitle($"Wartości kontrastu dla poszczególnych zestawień kolorów dominujących");

            if (model.AccessibilityModel.ColorAccessibilityModel.DominantColorsWithLowContrast.Count > 0)
            {
                _generator.AddText(
                    $"Dla wartości niższych niż {model.ImageProcessingParameters.contrastLowerThreshold}");
                _generator.AddDominantColorsContrastTable(model.AccessibilityModel.ColorAccessibilityModel
                    .DominantColorsWithLowContrast);
                _generator.AddLineBreak();
            }
            else
            {
                _generator.AddText(
                    $"Nie wykryto par kolorów, których contrast ratio wynosi mniej niż {model.ImageProcessingParameters.contrastLowerThreshold}");
            }

            if (model.AccessibilityModel.ColorAccessibilityModel.DominantColorsWithHighContrast.Count> 0)
            {
                _generator.AddText(
                    $"Dla wartości wyższych niż {model.ImageProcessingParameters.contrastUpperThreshold}");
                _generator.AddDominantColorsContrastTable(model.AccessibilityModel.ColorAccessibilityModel
                    .DominantColorsWithHighContrast);
                _generator.AddLineBreak();
            }
            else
            {
                _generator.AddText(
                    $"Nie wykryto par kolorów, których contrast ratio wynosi więcej niż {model.ImageProcessingParameters.contrastUpperThreshold}");
            }
        }

        private void DominantColorsReportPart(AnalysisModel model)
        {
            _generator.AddSubTitle("Detekcja Dominujących Kolorów");
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
            _generator.AddSubTitle("Optical Character Recognition (Wykrywanie Tekstu)");
            _generator.AddText("Korzystając z procesu OCR odczytano następujące słowa: ");
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
            _generator.AddText(
                "Obraz został podzielony na segmenty według kolorów, tak, aby każdy segment zawerał w przybliżeniu 2 kolory lub miał conajmniej rozmiary 10x10 pikseli.");

            var parameter = model.AnalysisParameters.GetParameter(FeatureType.SkipTextAnalysis);
            if (parameter is { isEnabled: false })
                _generator.AddText(
                    "Segmentacja została przeprowadzona z wyłączeniem obszarów zawierających tekst. Ponieważ obszary dzielone są za pomocą prostokątów może to prowadzić do sytuacji, w której znacząca ilość segmentów zostanie wykluczona. W takim przypadku zaleca się ponowne przeprowadzenie analizy z wyłączoną opcją pomijania obszarów zawierających tekst.");

            _generator.AddText("Poniżej przedstawiono wynik segmentacji:");
            var segmentationTexture = model.PreprocessedModel.SegmentedTextureModel.VisualizationMat.ToTexture();
            _generator.AddImage(segmentationTexture);
        }
    }
}