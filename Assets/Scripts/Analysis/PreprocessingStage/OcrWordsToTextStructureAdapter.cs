using System;
using System.Collections.Generic;
using UnityEngine;

namespace Analysis
{
    public class OcrWordsToTextStructureAdapter
    {
        public TextDocumentModel Convert(List<Box> detectedBoxes, List<string> detectedWords)
        {
            var words = ConvertToWords(detectedBoxes, detectedWords);
            var lines = ConvertToLines(words);
            var textBlocks = ConvertToTextBlock(lines);
            var textDocument = ConvertToTextDocument(textBlocks);
            return textDocument;
        }

        private TextDocumentModel ConvertToTextDocument(List<TextBlockModel> textBlocks)
        {
            var textDocument = new TextDocumentModel();
            foreach (var textBlock in textBlocks)
            {
                textDocument.AddTextBlock(textBlock);
            }

            return textDocument;
        }

        private List<TextBlockModel> ConvertToTextBlock(List<LineModel> lines)
        {
            var textBlocks = new List<TextBlockModel>();
            var currentBlock = new TextBlockModel();
            foreach (var line in lines)
            {
                if (currentBlock.Lines.Count == 0)
                {
                    currentBlock.AddLine(line);
                    continue;
                }

                var box = currentBlock.Lines[0].Words[0].Box;
                if (Mathf.Abs(line.Box.y - box.y) <= box.h * 2)
                {
                    currentBlock.AddLine(line);
                }
                else
                {
                    currentBlock.RevalidateBox();
                    textBlocks.Add(currentBlock);
                    currentBlock = new TextBlockModel();
                    currentBlock.AddLine(line);
                }
            }

            if (currentBlock.Lines.Count > 0)
            {
                currentBlock.RevalidateBox();
                textBlocks.Add(currentBlock);
            }

            return textBlocks;
        }

        private List<LineModel> ConvertToLines(List<WordModel> words)
        {
            var lines = new List<LineModel>();
            var currentLine = new LineModel();
            foreach (var word in words)
            {
                if (currentLine.Words.Count == 0)
                {
                    currentLine.AddWordToLine(word);
                    continue;
                }

                if (Mathf.Abs(word.Box.y - currentLine.Words[0].Box.y) < word.FontSize)
                {
                    currentLine.AddWordToLine(word);
                }
                else
                {
                    currentLine.RevalidateBox();
                    lines.Add(currentLine);
                    currentLine = new LineModel();
                    currentLine.AddWordToLine(word);
                }
            }

            if (currentLine.Words.Count > 0)
            {
                currentLine.RevalidateBox();
                lines.Add(currentLine);
            }

            return lines;
        }

        private List<WordModel> ConvertToWords(List<Box> detectedBoxes, List<string> detectedWords)
        {
            var words = new List<WordModel>();
            for (int i = 0; i < detectedBoxes.Count; i++)
            {
                words.Add(new WordModel
                {
                    Box = detectedBoxes[i],
                    Text = detectedWords[i],
                    FontSize = detectedBoxes[i].h,
                });
            }

            return words;
        }
    }
}