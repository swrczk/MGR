using System.Text;
using Analysis.AccessibilityProcessingStage.Model;
using Analysis.ReportGenerationStage.Model;
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

        public string Run(AnalysisModel model)
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

            SegmentationReportPart(model);
            SegmentationWithContrastReportPart(model);
            _generator.BreakPage();

            OcrReportPart(model);
            _generator.BreakPage();
            TextPropertiesReportPart(model);
            _generator.BreakPage();


            _generator.SaveCurrentPdf();
            return _generator.ReportPath;
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
            _generator.AddText("- wyznacznik,jak bardzo tekst na obrazie jest nieczytelny dla osób autystycznych.");
            _generator.AddLineBreak();

            _generator.AddText(
                $"Wynik skali dostępności dla parametrów kolorów: {model.AccessibilityModel.ColorScore}");
            _generator.AddText(
                "- wyznacznik,jak bardzo kolory na obrazie są nieczytelne dla osób z zaburzeniami widzenia.");
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

            if (model.AccessibilityModel.ColorAccessibilityModel.DominantColorsWithHighContrast.Count > 0)
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

            var text = "Wykryte słowa zostały zaznaczone na obrazie:";
            var ocrResultHighlightedTexture = model.PreprocessedModel.OcrResult.HighlightedTexture;
            _generator.AddTextWithImageBelow(text, ocrResultHighlightedTexture);

            _generator.AddText(
                "Model może być niedokładny. Jeżeli wynik wykrywania tekstu nie jest zadowalający (wykryte mniej niż 50% treści) należy powtórzyć analizę.");
            var ocrErrorsText = "Możliwe błędy OCR:\n" +
                                "- Algorytm może wykryć mniej niż 50% treści, co oznacza, że wiele słów może być pominiętych.\n" +
                                "- Algorytm może błędnie rozpoznać niektóre słowa, szczególnie jeśli są one zniekształcone lub niewyraźne.";

            _generator.AddText(ocrErrorsText);
            _generator.AddLineBreak();
        }


        private void TextPropertiesReportPart(AnalysisModel model)
        {
            _generator.AddSubTitle("Analiza właściwości wykrytego tekstu");
            
            var textLines = new string[]
            {
                "Analizowane Elementy:",
                "1. Dominant Colors With Low Contrast (Dominujące kolory o niskim kontraście)",
                $"   - Ocena, jak wiele dominujących kolorów ma kontrast niższy niż {model.ImageProcessingParameters.contrastLowerThreshold}.",
                "   - Możliwe błędy: Algorytm może nie rozpoznać wszystkich kolorów prawidłowo, zwłaszcza jeśli są bardzo podobne. Także biało - czarny tekst wykazuje się niskim kontrastem, pomimo dużej czytelności",
                "2. Dominant Colors With High Contrast (Dominujące kolory o wysokim kontraście)",
                $"   - Ocena, jak wiele dominujących kolorów ma   kontrast wyższy niż {model.ImageProcessingParameters.contrastLowerThreshold}.",
                "   - Możliwe błędy: Algorytm może nie zauważyć wszystkich kontrastujących kolorów.",
                "3. Segments With Low Contrast (Segmenty o niskim kontraście)",
                "   - Ocena, jaka część obrazu ma niskie kontrasty.",
                "   - Możliwe błędy: Algorytm może nieprawidłowo ocenić kontrast w segmentach, szczególnie w skomplikowanych teksturach.",
                "4. Segments With High Contrast (Segmenty o wysokim kontraście)",
                "   - Ocena, jaka część obrazu ma wysokie kontrasty.",
                "   - Możliwe błędy: Algorytm może nieprawidłowo ocenić kontrast w segmentach.",
                "5. High Contrast Text Colors (Kolory tekstu o wysokim kontraście)",
                "   - Ocena, czy kolory tekstu mają wysoki kontrast.",
                "   - Możliwe błędy: Algorytm może błędnie ocenić kontrast, szczególnie w przypadku czarno-białego tekstu.",
                "6. Low Contrast Text Colors (Kolory tekstu o niskim kontraście)",
                "   - Ocena, czy kolory tekstu mają niski kontrast.",
                "   - Możliwe błędy: Algorytm może błędnie ocenić kontrast tekstu.",
                "7. Image As Text Background (Obraz jako tło tekstu)",
                "   - Ocena, czy tekst jest na tle obrazu, co może utrudniać jego czytelność.",
                "   - Możliwe błędy: Algorytm może błędnie wykryć tło obrazu.",
                "8. Too Small Font Size (Zbyt mała wielkość czcionki)",
                "   - Ocena, czy czcionka jest wystarczająco duża, aby była czytelna.",
                "   - Możliwe błędy: Algorytm może błędnie ocenić wielkość czcionki.",
                "9. Too Small Interline In Text Block (Zbyt mały odstęp między wierszami)",
                "   - Ocena, czy odstęp między wierszami tekstu jest wystarczający.",
                "   - Możliwe błędy: Algorytm może błędnie ocenić odstęp między wierszami.",
                "10. Right Aligned Text (Tekst wyrównany do prawej)",
                "    - Ocena, czy tekst jest wyrównany do prawej, co może utrudniać jego czytanie.",
                "    - Możliwe błędy: Algorytm może błędnie wykryć wyrównanie tekstu."
            };
            
            var stringBuilder = new StringBuilder();
            foreach (var line in textLines)
            {
                stringBuilder.Append(line);
                stringBuilder.Append("\n");
            }
            _generator.AddText(stringBuilder.ToString());

            AddTextAccessibilityIndicator("High Contrast Text Colors",
                model.AccessibilityModel.TextAccessibilityModel.HighContrastTextColors);
            AddTextAccessibilityIndicator("Low Contrast Text Colors",
                model.AccessibilityModel.TextAccessibilityModel.LowContrastTextColors);
            AddTextAccessibilityIndicator("Image As Text Background",
                model.AccessibilityModel.TextAccessibilityModel.ImageAsTextBackground);
            AddTextAccessibilityIndicator("Too Small Font Size",
                model.AccessibilityModel.TextAccessibilityModel.TooSmallFontSize);
            AddTextAccessibilityIndicator("Too Small Interline In Text Block",
                model.AccessibilityModel.TextAccessibilityModel.TooSmallInterlineInTextBlock);
            AddTextAccessibilityIndicator("Right Aligned Text",
                model.AccessibilityModel.TextAccessibilityModel.RightAlignedText);

            _generator.AddLineBreak();
            var textAnalysisErrorsText = "Możliwe błędy analizy tekstu:\n" +
                                         "- algorytm może błędnie ocenić kontrast tekstu, szczególnie w przypadku czarno-białego tekstu.\n" +
                                         "- algorytm może błędnie wykryć tło obrazu, co wpływa na czytelność tekstu.\n" +
                                         "- algorytm może błędnie ocenić wielkość czcionki i odstępy między wierszami, co wpływa na ogólną czytelność tekstu.\n" +
                                         "- algorytm może błędnie wykryć wyrównanie tekstu, co może wprowadzać w błąd.";
            _generator.AddText(textAnalysisErrorsText);
        }

        private void AddTextAccessibilityIndicator(string title, AccessibilityIndicatorModel indicator)
        {
            _generator.AddSubTitle( $"{title}: ");
            var text = $"Procent zajmowanej powierzchni: {indicator.BaseValue}% \n Waga: {indicator.Weight}";
            var texture2D = indicator.Visualization.ToTexture();
            _generator.AddTextWithImageBelow(text, texture2D);
            _generator.AddLineBreak();
        }

        private void SegmentationReportPart(AnalysisModel model)
        {
            _generator.AddSubTitle("Segmentacja");
            _generator.AddText(
                "Obraz został podzielony na segmenty według kolorów, tak, aby każdy segment zawerał w przybliżeniu 2 kolory lub miał conajmniej rozmiary 10x10 pikseli.");

            var parameter = model.AnalysisParametersSo.GetParameter(FeatureType.SkipTextAnalysis);
            if (parameter is { isEnabled: false })
                _generator.AddText(
                    "Segmentacja została przeprowadzona z wyłączeniem obszarów zawierających tekst. Ponieważ obszary dzielone są za pomocą prostokątów może to prowadzić do sytuacji, w której znacząca ilość segmentów zostanie wykluczona. W takim przypadku zaleca się ponowne przeprowadzenie analizy z wyłączoną opcją pomijania obszarów zawierających tekst.");

            _generator.AddText("Poniżej przedstawiono wynik segmentacji:");
            var segmentationTexture = model.PreprocessedModel.SegmentedTextureModel.VisualizationMat.ToTexture();
            _generator.AddImage(segmentationTexture);
        }

        private void SegmentationWithContrastReportPart(AnalysisModel model)
        {
            _generator.AddSubTitle("Analiza kontrastu segmentów");
            _generator.AddText(
                "Każdy z segmentów został poddany analizie kontrastu. Zostały wyodrębnione segmenty o niskim i wysokim kontraście. Poniżej przedstawiono wynik analizy:");
            _generator.AddLineBreak();

            _generator.AddText(
                $"Procent przestrzeni dla segmentów o niskim kontraście: {model.AccessibilityModel.ColorAccessibilityModel.SegmentsWithLowContrast.BaseValue}%");
            _generator.AddText(
                $"Procent przestrzeni dla segmentów o wysokim kontraście: {model.AccessibilityModel.ColorAccessibilityModel.SegmentsWithHighContrast.BaseValue}%");
            _generator.AddLineBreak();


            var text = "czerwony - segmenty o niskim kontraście \n niebieski - segmenty o wysokim kontraście";
            var texture = model.PropertiesModel.ContrastVisualizationMat.ToTexture();
            _generator.AddTextWithImageBelow(text, texture);
            _generator.AddLineBreak();

            _generator.AddText(
                "Suma powierzchni segmentów o niskim lub wysokim kontraście bezprośrednio wpływa na parametr dostępności");
        }
    }
}