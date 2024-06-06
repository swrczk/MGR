using System.Collections.Generic;
using Analysis.PropertiesProcessingStage.Model;

namespace Analysis
{
    public class Margins
    {
        public float Top { get; set; }
        public float Bottom { get; set; }
        public float Left { get; set; }
        public float Right { get; set; }
    }
    public class TextDocumentModel
    {
        public List<TextBlockModel> TextBlocks { get; } = new();
        public int WordCount { get; private set; }
        public int LineCount { get; private set; }
        public int TextBlockCount { get; private set; }
        public Dictionary<(int, int), BlockDistanceInfoModel> BlockDistances { get; set; }
        public Margins Margins { get; set; } = new(); 

        public void AddTextBlock(TextBlockModel textBlockModel)
        {
            TextBlocks.Add(textBlockModel);
            TextBlockCount++;
            LineCount += textBlockModel.Lines.Count;
            foreach (var line in textBlockModel.Lines)
            {
                WordCount += line.Words.Count;
            }
        }

        public List<WordModel> GetWords()
        {
            var words = new List<WordModel>();
            foreach (var textBlock in TextBlocks)
            {
                foreach (var line in textBlock.Lines)
                {
                    words.AddRange(line.Words);
                }
            }

            return words;
        }

        public List<LineModel> GetLines()
        {
            var lines = new List<LineModel>();
            foreach (var textBlock in TextBlocks)
            {
                lines.AddRange(textBlock.Lines);
            }

            return lines;
        }
    }
}