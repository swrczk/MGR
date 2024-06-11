using System;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using UnityEngine;
using Color = iTextSharp.text.Color;
using Font = iTextSharp.text.Font;

public class PDFGenerator
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

    public void AddTextureWithDescription(Texture2D texture, Phrase title, Paragraph description)
    {
        var table = new PdfPTable(2);
        if (texture != null)
        {
            var imageBytes = texture.EncodeToPNG();
            var image = iTextSharp.text.Image.GetInstance(imageBytes);
            var imageCell = new PdfPCell(image, true);
            table.AddCell(imageCell);
        }

        title.Font = font;
        description.Font = font;
        var textCell = new PdfPCell(title);
        textCell.AddElement(description);
        table.AddCell(textCell);

        document.Add(table);
    }

    public void BreakPage()
    {
        document.NewPage();
    }

    public void SaveCurrentPdf()
    {
        document.Close();
        Debug.LogError("PDF  w: " + path);
    }
}