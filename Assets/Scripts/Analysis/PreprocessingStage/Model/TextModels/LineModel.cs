using System.Collections.Generic;

namespace Analysis
{
    public class LineModel:ITextModel
    {
        public List<WordModel> Words { get;  } = new();
        public Box Box { get; set; }
        public float AverageFontSize { get; set; }
        public float AverageSpacing{ get; set; }
        public float ImageAreaPercentage { get; set; }
    }
}