using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Analysis
{
    public static class OcrWordsToTextStructureAdapter
    {
        public static TextDocument Convert(List<Box> detectedBoxes, List<string> detectedWords)
        {
            var words = ConvertToWords(detectedBoxes, detectedWords);
            var lines = ConvertToLines(words);
            var textBlocks = ConvertToTextBlock(lines);
            var textDocument = ConvertToTextDocument(textBlocks);
            return textDocument;
        }

        private static TextDocument ConvertToTextDocument(List<TextBlock> textBlocks)
        {
            var textDocument = new TextDocument();
            foreach (var textBlock in textBlocks)
            {
                textDocument.AddTextBlock(textBlock);
            }

            return textDocument;
        }

        private static List<TextBlock> ConvertToTextBlock(List<Line> lines)
        {
            var textBlocks = new List<TextBlock>();
            var currentBlock = new TextBlock();
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
                    currentBlock = new TextBlock();
                    currentBlock.AddLine(line);
                }
            }
            
            if(currentBlock.Lines.Count > 0)
            {
                currentBlock.RevalidateBox();
                textBlocks.Add(currentBlock);
            }

            return textBlocks;
        }

        private static List<Line> ConvertToLines(List<Word> words)
        {
            var lines = new List<Line> ();
            var currentLine = new Line();
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
                    currentLine = new Line();
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

        private static List<Word> ConvertToWords(List<Box> detectedBoxes, List<string> detectedWords)
        {
            var words = new List<Word>();
            for (int i = 0; i < detectedBoxes.Count; i++)
            {
                words.Add( new Word
                {
                    Box = detectedBoxes[i],
                    Text = detectedWords[i],
                    FontSize = detectedBoxes[i].h,
                });
            }

            return words;
        }
    }

    [Serializable]
    public class TextDocument
    {
        public List<TextBlock> TextBlocks { get; set; } = new List<TextBlock>();
        public int WordCount { get; private set; }
        public int LineCount { get; private set; }
        public int TextBlockCount { get; private set; }

        public void AddTextBlock(TextBlock textBlock)
        {
            TextBlocks.Add(textBlock);
            TextBlockCount++;
            LineCount += textBlock.Lines.Count;
            foreach (var line in textBlock.Lines)
            {
                WordCount += line.Words.Count;
            }
        }
        
        public List<Word> GetWords()
        {
            var words = new List<Word>();
            foreach (var textBlock in TextBlocks)
            {
                foreach (var line in textBlock.Lines)
                {
                    words.AddRange(line.Words);
                }
            }

            return words;
        }
        
        public List<Line> GetLines()
        {
            //get all lines not using linq
            var lines = new List<Line>();
            foreach (var textBlock in TextBlocks)
            {
                lines.AddRange(textBlock.Lines);
            }

            return lines;
        }
    }

    [Serializable]
    public class TextBlock
    {
        public List<Line> Lines { get; set; } = new List<Line>();
        public Box Box { get; set; } = new Box();
        public float AverageFontSize { get; set; }
        // public float AverageFontColor { get; set; }
        // public float AverageFontBackground { get; set; }
        // public float AverageDistanceBetweenLines { get; set; }

        public void AddLine(Line line)
        {
            Lines.Add(line);
        }

        public void RevalidateBox()
        {
            AverageFontSize = (float)Lines.Average(l => l.AverageFontSize);
            Box = new Box
            {
                x = Lines[0].Box.x,
                y = Lines[0].Box.y,
                w = Lines[^1].Box.x + Lines[^1].Box.w - Lines[0].Box.x,
                h = Lines[0].Box.h
            };
        }
    }

    [Serializable]
    public class Line
    {
        public List<Word> Words { get; set; } = new List<Word>();
        public Box Box { get; private set; }
        public float AverageFontSize { get; private set; }

        public void AddWordToLine(Word word)
        {
            Words.Add(word);
        }

        public void RevalidateBox()
        {
            AverageFontSize = (float)Words.Average(w => w.FontSize);
            Box = new Box
            {
                x = Words[0].Box.x,
                y = Words[0].Box.y,
                w = Words[^1].Box.x + Words[^1].Box.w - Words[0].Box.x,
                h = Words[0].Box.h
            };
        }
    }

    [Serializable]
    public class Word
    {
        public Box Box { get; set; } = new Box();
        public string Text { get; set; }
        public int FontSize { get; set; }
        public Color TextColor { get; set; }

        public Color TextBackgroundColor { get; set; }
        public bool IsUniformBackground { get; set; }
        public void SaveTextColor(Color color)
        {
            TextColor = color;
        }

        public void SaveBackgroundColor(Color color, bool isUniform)
        {
            TextBackgroundColor = color;
            IsUniformBackground = isUniform;
        }
    }
}