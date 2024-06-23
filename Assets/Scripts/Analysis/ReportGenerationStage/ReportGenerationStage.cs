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
            if (model.PreprocessedModel.OcrResult.Text.Count > 20)
            {
                
                TextPropertiesReportPart(model);
                _generator.BreakPage();
            }
            else
            {
                
                _generator.AddSubTitle("Analiza właściwości wykrytego tekstu");
                _generator.AddSubTitle("Ilość wykrytych znaków jest niewystarczajaca, aby przeprowadzić analizę właściwości tekstu.");
            }


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
            _generator.AddText(
                $"W cel zwiększenia dostępności należy dążyć do zminimalizowania wartości skali dostępności.");

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
            _generator.AddSubTitle(
                $"Wartości współczynnika kontrastu dla poszczególnych zestawień kolorów dominujących");

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
            _generator.AddText(
                "Dominujące kolory określają palete 5 kolorów, które zajmują największą powierzchnię interfejsu");
            _generator.AddText("Dominujące kolory na ekranie to: ");
            _generator.AddImage(colorsMat.ToTexture());
            _generator.AddText(
                "Powyższy obrazek przedstawia segmentacje według kolorów dominujących. Kolory o mniejszej częstotliwości występowania są pomijane.");
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
                "Specyfika modelu może powodować nieprecyzyjne wyniki. Jeżeli wynik wykrywania tekstu nie jest zadowalający należy powtórzyć analizę.");
            var ocrErrorsText = "Możliwe błędy procesu OCR:\n" +
                                "- wykrycie mniej niż 50% treści: Warto zwrócić uwage na jakość tekstury. Specyfika modelu sprawia, że wynik może być inny przy każdej iteracji, dlatego warto spróbować podejsć do analizy jeszcze raz.\n" +
                                "- błędne rozpoznanie znaczenia słowa,\n" +
                                "- wykrycie znaków lub liter w miejscach, gdzie nie występują.";

            _generator.AddText(ocrErrorsText);
            _generator.AddLineBreak();
        }


        private void TextPropertiesReportPart(AnalysisModel model)
        {
            _generator.AddSubTitle("Analiza właściwości wykrytego tekstu");

            var textLines = new string[]
            {
                "Progi analizy wspólczynnika kontrastu (przyjmującego wartości od 1 do 21):",
                $"   - dolny próg: {model.ImageProcessingParameters.contrastLowerThreshold}:1, pokrywający się z wytycznymi WCAG AA",
                $"   - górny próg: {model.ImageProcessingParameters.contrastUpperThreshold}:1, dobrany eksperymentalnie z powodu braku wytycznych.\n",
                "Analizowane Elementy:",
                "1. Dominujące kolory o niskim/wysokim kontraście",
                "   - dla 5 najczęściej występujących barw obliczany jest współczynnik kontrastu wg. wzoru WCAG,",
                "   - obrana metodologia oceny kolorów nie jest idealna. Istnieje wiele przykładów par kolorów, które są bardzo czytelne, ale nie spełniają kryteriów kontrastu, oraz odwrotnie. Warto przy analizie porównać, jak wskazane kolory koegzystują ze sobą, i czy są często ze sobą zestawiane.",
                "   - algorytm porównuje ze sobą wszystkie wyznaczone kolory dominujące, traktując ją jako obraną palete barw. Warto zwrócić uwagę, czy kolory nie spełniające kryteriów analizy kontrastu nie są przypadkiem używane w innych miejscach interfejsu, nie sąsiądując ze sobą.",
                "\n3. Segmenty o niskim/wysokim kontraście",
                "   - obraz po wykluczeniu tekstu został podzielony na segmenty przy pomocy wyszukiwania binarnego. Każdy segment zawiera w przybliżeniu 2 kolory lub ma conajmniej rozmiary 10x10 pikseli. Następnie każdy segment został poddany analizie kontrastu oraz porównany do wartości progowych.",
                "   - algorytm analizuje każdy segment osobno, nie biorąc pod uwagę kontekstu, w jakim się znajduje. Warto zwrócić uwagę, czy segmenty o niskim kontraście nie sąsiadują z segmentami o wysokim kontraście, co może wprowadzać w błąd.",
                "   - wybrana metoda dzielenia segmentów napotyka trudności w przypadku obrazów z gradientami lub zawierających obiekty 3D",
                "\n4. Wysoki/niski kontrast między kolorem tła a tekstem",
                "   - dla wykrytych bloków tekstu przez OCR wyznaczany jesk kolor tekstu i tła za pomocą algorytmu do wyznaczania kolorów dominujących. Następnie obliczany jest współczynnik kontrastu wg. wzoru WCAG.",
                "   - algorytm uwzględnia skrajne przypadki, tj. słowo jednoliterowe, bądź mały kontrast między kolorami",
                "\n5. Użycie obrazu jako tła tekstu",
                "   - przy analzie kolorów z punkt 4, wyznaczana jest histogram entropii bloku tekstu. W przypadku, gdy tło ma większą entropię niż tekst, uznawane jest za obrazowe.",
                "   - nieskuteczny przy czcionkach o dużej ilości detali, np. czcionki ozdobne",
                "\n6. Wielkość czcionki mniejsza niż 14px",
                "   - wartość wytycznych WCAG AA, sprawdzana na podstawie wysokości każdego bloku wykrytego przez OCR. Słowa są łączone w linie tekstu na podstawie odległości między nimi. Następnie obliczana jest wysokość czcionki jako średnia wysokość słów w linii.",
                "   - procentowe pokrycie tekstu przez OCR ma bezpośredni wpływ na wynik. Warto zwrócić uwagę, czy wykryto conajmniej 50% tekstu na obrazie.",
                "\n7. Interlinia mniejsza niż 1,5x wysokość czcionki",
                "   - wartość wytycznych WCAG AA. Linie są łaczone w bloki tekstu na podstawie odległości między nimi. Następnie obliczana jest wysokość czcionki jako średnia wysokość linii w bloku tekstu.",
                "   - procentowe pokrycie tekstu przez OCR ma bezpośredni wpływ na wynik. Warto zwrócić uwagę, czy wykryto conajmniej 50% tekstu na obrazie.",
                "\n8. Tekst wyrównany do prawej krawędzi",
                "    - wg publikacji tekst wyrównany do prawej krawędzi jest najmniej czytelny. Po wyznaczeniu słów, lini tekstu i bloków tekstu porównywane są ich współrzędne prawej dolnej krawędzi. W przypadku, gdy współrzędne są zbliżone do siebie, tekst jest uznawany za wyrównany do prawej krawędzi.",
                "    - nieskuteczne w przypadku elementów tj. menu, gdzie elementy zawierają pojedzyńcze hasło w linii.",
            };

            var stringBuilder = new StringBuilder();
            foreach (var line in textLines)
            {
                stringBuilder.Append(line);
                stringBuilder.Append("\n");
            }

            _generator.AddText(stringBuilder.ToString());

            AddTextAccessibilityIndicator("Wysoki kontrast między kolorem tła a tekstem",
                model.AccessibilityModel.TextAccessibilityModel.HighContrastTextColors);
            AddTextAccessibilityIndicator("Niski kontrast między kolorem tła a tekstem",
                model.AccessibilityModel.TextAccessibilityModel.LowContrastTextColors);
            AddTextAccessibilityIndicator("Użycie obrazu jako tła tekstu",
                model.AccessibilityModel.TextAccessibilityModel.ImageAsTextBackground);
            AddTextAccessibilityIndicator("Wielkość czcionki mniejsza niż 14px",
                model.AccessibilityModel.TextAccessibilityModel.TooSmallFontSize);
            AddTextAccessibilityIndicator("Interlinia mniejsza niż 1,5x wysokość czcionki",
                model.AccessibilityModel.TextAccessibilityModel.TooSmallInterlineInTextBlock);
            AddTextAccessibilityIndicator("Tekst wyrównany do prawej krawędzi",
                model.AccessibilityModel.TextAccessibilityModel.RightAlignedText);

            _generator.AddLineBreak();
            var textAnalysisErrorsText = "Krańcowe przypadki błędnie interpretowane przez algorytm:\n" +
                                         "- kontrast tekstu spoza akceptowalnej wartości dla bardzo czytelnego tekstu, szczególnie w przypadku czarno-białego tekstu.\n" +
                                         "- błedne wykrycie tła tekstu przy czcionkach ozdobnych lub wypełnionych gradientem,\n" +
                                         "- przy niskim procencie pokrycia tekstu przez OCR algorytm może błędnie wykryć wyrównanie tekstu";
            _generator.AddText(textAnalysisErrorsText);
        }

        private void AddTextAccessibilityIndicator(string title, AccessibilityIndicatorModel indicator)
        {
            _generator.AddSubTitle($"{title}: ");
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