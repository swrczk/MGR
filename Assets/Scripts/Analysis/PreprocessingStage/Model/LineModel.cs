using System.Collections.Generic;
using System.Linq;

namespace Analysis
{
    public class LineModel
    {
        public List<WordModel> Words { get; set; } = new List<WordModel>();
        public Box Box { get; private set; }
        public float AverageFontSize { get; private set; }

        public void AddWordToLine(WordModel wordModel)
        {
            Words.Add(wordModel);
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
}