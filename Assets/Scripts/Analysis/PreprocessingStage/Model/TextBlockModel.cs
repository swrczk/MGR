using System.Collections.Generic;
using System.Linq;

namespace Analysis
{
    public class TextBlockModel
    {
        public List<LineModel> Lines { get; set; } = new List<LineModel>();
        public Box Box { get; set; } = new Box();

        public float AverageFontSize { get; set; }
        // public float AverageFontColor { get; set; }
        // public float AverageFontBackground { get; set; }
        // public float AverageDistanceBetweenLines { get; set; }

        public void AddLine(LineModel lineModel)
        {
            Lines.Add(lineModel);
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
}