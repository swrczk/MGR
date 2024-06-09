using System.Collections.Generic;
using System.Linq;

namespace Analysis
{
    public class TextBlockModel:ITextModel
    {
        public List<LineModel> Lines { get; } = new();
        public Box Box { get;   set; }
        public float AverageFontSize { get; set; }
        public float AverageInterline { get; set; }
        public bool IsLeftAligned { get; set; }
        public float ImageAreaPercentage { get; set; }
        // public float AverageFontColor { get; set; }
        // public float AverageFontBackground { get; set; }
        
        public List<WordModel> GetWords()
        {
            return Lines.SelectMany(line => line.Words).ToList();
        }
 
    }
}