using System;
using System.Collections.Generic;
using System.IO;
using Analysis.AccessibilityProcessingStage.Model;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using UnityEngine;
using Color = iTextSharp.text.Color;
using Font = iTextSharp.text.Font;

namespace Analysis.ReportGenerationStage.Processor
{
    public class PdfGenerator
    {
        private Document document;
        private string path;
        private float pageWidth;
        private Font font;
        private Font titleFont;
        private Font subtitleFont;
        private BaseFont baseFont;

        public void Create(string screenName, string fileName)
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var directoryPath = Path.Combine(documentsPath, "AccessibilityAnalysisRaports");
            path = Path.Combine(directoryPath, $"AccessibilityAnalysisRaport_{screenName}_{fileName}.pdf");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            document = new Document();
            PdfWriter.GetInstance(document, new FileStream(path, FileMode.Create));
            document.Open();
            pageWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin;

            var fontPath = Path.Combine(Application.dataPath, "Fonts", "Roboto", "Roboto-Thin.ttf");
            baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            font = new Font(baseFont, 12, Font.NORMAL);
            titleFont = new Font(baseFont, 18, Font.BOLD);
            subtitleFont = new Font(baseFont, 16, Font.BOLDITALIC);
        }

        public void SaveCurrentPdf()
        {
            document.Close();
            Debug.Log("PDF  w: " + path);
        }

        public void AddTitle(string title)
        {
            var titleParagraph = new Paragraph(title, titleFont)
            {
                SpacingBefore = 20f,
                SpacingAfter = 10f,
            };
            document.Add(titleParagraph);
        }

        public void AddSubTitle(string subTitle)
        {
            var subTitleParagraph = new Paragraph(subTitle, subtitleFont)
            {
                SpacingBefore = 10f,
                SpacingAfter = 10f,
            };
            document.Add(subTitleParagraph);
        }

        public void AddText(string text)
        {
            var paragraph = new Paragraph(text, font)
            {
                SpacingBefore = 5f,
                SpacingAfter = 5f,
            };
            document.Add(paragraph);
        }

        public void AddTextWithImageBelow(string text, Texture2D texture)
        {
            var paragraph = new Paragraph(text, font)
            {
                SpacingAfter = 10f
            };
            document.Add(paragraph);

            var imageBytes = texture.EncodeToPNG();
            var image = iTextSharp.text.Image.GetInstance(imageBytes);
            if (image.Height > document.PageSize.Height - document.TopMargin - document.BottomMargin)
            {
                image.ScaleToFit(document.PageSize.Width - document.LeftMargin - document.RightMargin,
                    document.PageSize.Height - document.TopMargin - document.BottomMargin);
            }

            var table = new PdfPTable(1)
            {
                WidthPercentage = 100
            };
            var cell = new PdfPCell(image)
            {
                Border = Rectangle.NO_BORDER,
                PaddingTop = 0,
                PaddingBottom = 10f
            };
            table.AddCell(cell);

            // Ensure the text and image stay together on the same page
            table.KeepTogether = true;
            document.Add(table);
        }

        public void AddLineSeparator()
        {
            var lineSeparator = new LineSeparator(1f, 100f, Color.BLACK, Element.ALIGN_CENTER, -1);
            document.Add(lineSeparator);
        }

        public void AddLineBreak()
        {
            document.Add(new Paragraph(" "));
        }

        public void AddImage(Texture2D texture)
        {
            var imageBytes = texture.EncodeToPNG();
            var image = iTextSharp.text.Image.GetInstance(imageBytes);
            image.ScaleToFit(pageWidth, image.Height);
            image.SpacingAfter = 20f;
            image.SpacingBefore = 20f;
            document.Add(image);
        }

        public void AddTextureWithDescription(Texture2D texture, Paragraph description)
        {
            var table = new PdfPTable(2);
            if (texture != null)
            {
                var imageBytes = texture.EncodeToPNG();
                var image = iTextSharp.text.Image.GetInstance(imageBytes);
                var imageCell = new PdfPCell(image, true);
                table.AddCell(imageCell);
            }

            description.Font = font;
            var textCell = new PdfPCell();
            textCell.AddElement(description);
            table.AddCell(textCell);

            document.Add(table);
        }

        public void BreakPage()
        {
            document.NewPage();
        }


        public void AddAccessibilityIndicatorsResultTable(AccessibilityModel model)
        {
            var table = new PdfPTable(3);
            table.WidthPercentage = 100;

            // Dodawanie nagłówków tabeli
            table.AddCell(new PdfPCell(new Phrase("Name", subtitleFont)));
            table.AddCell(new PdfPCell(new Phrase("Base Value", subtitleFont)));
            table.AddCell(new PdfPCell(new Phrase("Weight", subtitleFont)));

            AddIndicatorToTable(table, "Dominant Colors With Low Contrast",
                model.ColorAccessibilityModel.OverallColorLowContrast);
            AddIndicatorToTable(table, "Dominant Colors With High Contrast",
                model.ColorAccessibilityModel.OverallColorContrastHigh);
            AddIndicatorToTable(table, "Segments With Low Contrast",
                model.ColorAccessibilityModel.SegmentsWithLowContrast);
            AddIndicatorToTable(table, "Segments With High Contrast",
                model.ColorAccessibilityModel.SegmentsWithHighContrast);
            AddIndicatorToTable(table, "High Contrast Text Colors",
                model.TextAccessibilityModel.HighContrastTextColors);
            AddIndicatorToTable(table, "Low Contrast Text Colors", model.TextAccessibilityModel.LowContrastTextColors);
            AddIndicatorToTable(table, "Image As Text Background", model.TextAccessibilityModel.ImageAsTextBackground);
            AddIndicatorToTable(table, "Too Small Font Size", model.TextAccessibilityModel.TooSmallFontSize);
            AddIndicatorToTable(table, "Too Small Interline In Text Block",
                model.TextAccessibilityModel.TooSmallInterlineInTextBlock);
            AddIndicatorToTable(table, "Right Aligned Text", model.TextAccessibilityModel.RightAlignedText);

            document.Add(table);
        }

        private void AddIndicatorToTable(PdfPTable table, string name, AccessibilityIndicatorModel model)
        {
            table.AddCell(new PdfPCell(new Phrase(name, font)));
            table.AddCell(new PdfPCell(new Phrase(model.BaseValue.ToString(), font)));
            table.AddCell(new PdfPCell(new Phrase(model.Weight.ToString(), font)));
        }

        public void AddDominantColorsContrastTable(List<DominantColorAccessibilityIndicatorModel> values)
        {
            foreach (var model in values)
            {
                AddTextureWithDescription(model.Visualization.ToTexture(),
                    new Paragraph($"Procent zajmowanej przestrzeni: {model.BaseValue}% \n Kontrast: {model.Contrast}"));
            }
        }
    }
}