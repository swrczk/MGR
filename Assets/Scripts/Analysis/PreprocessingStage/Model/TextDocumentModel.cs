using System.Collections.Generic;

namespace Analysis
{
    public class TextDocumentModel
    {
        public List<TextBlockModel> TextBlocks { get; set; } = new List<TextBlockModel>();
        public int WordCount { get; private set; }
        public int LineCount { get; private set; }
        public int TextBlockCount { get; private set; }

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
            //get all lines not using linq
            var lines = new List<LineModel>();
            foreach (var textBlock in TextBlocks)
            {
                lines.AddRange(textBlock.Lines);
            }

            return lines;
        }
    }
}