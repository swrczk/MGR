using System.Collections.Generic;
using UnityEngine;

namespace Analysis
{
    public static class OcrWordsToTextStructureAdapter
    {
        public static TextBlock[] ConvertToTextBlock(List<Box> detectedBoxes, List<string> detectedWords)
        {
            Word[] words = ConvertToWords(detectedBoxes, detectedWords);
            Line[] lines = ConvertToLines(words);
            TextBlock[] textBlocks = ConvertLinesToTextBlock(lines);
            return textBlocks;
        }

        private static TextBlock[] ConvertLinesToTextBlock(Line[] lines)
        {
            List<TextBlock> textBlocks = new List<TextBlock>();
            TextBlock currentBlock = new();
            foreach (var line in lines)
            {
                if(currentBlock.Lines.Count == 0)
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
                }
            }

            return textBlocks.ToArray();
        }

        private static Line[] ConvertToLines(Word[] words)
        {
            List<Line> lines = new List<Line>();
            Line currentLine = new();
            foreach (var word in words)
            {
                if(currentLine.Words.Count == 0)
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
            if(currentLine.Words.Count > 0)
            {
                currentLine.RevalidateBox();
                lines.Add(currentLine);
            }

            return lines.ToArray();
        }

        private static Word[] ConvertToWords(List<Box> detectedBoxes, List<string> detectedWords)
        {
            Word[] words = new Word[detectedBoxes.Count];
            for (int i = 0; i < detectedBoxes.Count; i++)
            {
                words[i] = new Word
                {
                    Box = detectedBoxes[i],
                    Text = detectedWords[i],
                    FontSize = detectedBoxes[i].h,
                };
            }

            return words;
        }
    }

    public class Word
    {
        public Box Box { get; set; } = new Box();
        public string Text { get; set; }
        public int FontSize { get; set; }
        // public Color TextColor { get; set; }
        // public bool IsImageBackground { get; set; }
    }

    public class Line
    {
        public List<Word> Words { get; set; } = new List<Word>();
        public Box Box { get; private set; } = new();
        
        

        public void AddWordToLine(Word word)
        {
            Words.Add(word);
            // Words.Sort((w1, w2) => w1.Box.x.CompareTo(w2.Box.x));
        }
        
        public void RevalidateBox()
        {
            Box = new Box
            {
                x = Words[0].Box.x,
                y = Words[0].Box.y,
                w = Words[^1].Box.x + Words[^1].Box.w - Words[0].Box.x,
                h = Words[0].Box.h
            };
        }
    }

    public class TextBlock
    {
        public List<Line> Lines { get; set; } = new List<Line>();
        public Box Box { get; set; } = new Box();
        // public float AverageFontSize { get; set; }
        // public float AverageFontColor { get; set; }
        // public float AverageFontBackground { get; set; }
        // public float AverageDistanceBetweenLines { get; set; }
        
        public void AddLine(Line line)
        {
            Lines.Add(line);
        }
        
        public void RevalidateBox()
        {
            Box = new Box
            {
                x = Lines[0].Box.x,
                y = Lines[0].Box.y,
                w = Lines[^1].Box.x + Lines[^1].Box.w - Lines[0].Box.x,
                h = Lines[0].Box.h
            };
        }
    }
}