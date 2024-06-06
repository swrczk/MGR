using System.Collections.Generic;
using System.Linq;

namespace Analysis
{
    public class TextBlockPropertiesProcessor
    {
        public void ProcessLineProperties(List<TextBlockModel> blocks)
        {
            foreach (var block in blocks)
            {
                block.Box = CalculateBoxSize(block);
                block.AverageFontSize = CalculateAverageFontSize(block);
                block.AverageInterline = CalculateAverageLineSpacing(block.Lines);
            }
        }

        private float CalculateAverageLineSpacing(List<LineModel> lines)
        {
            if (lines.Count < 2)
            {
                return 0;
            }

            var distances = new List<float>();

            for (var i = 0; i < lines.Count - 1; i++)
            {
                float distance = lines[i + 1].Box.y - (lines[i].Box.y + lines[i].Box.h);
                distances.Add(distance);
            }

            return distances.Average();
        }

        private Box CalculateBoxSize(TextBlockModel block)
        {
            return new Box
            {
                x = block.Lines[0].Box.x,
                y = block.Lines[0].Box.y,
                w = block.Lines[^1].Box.x + block.Lines[^1].Box.w - block.Lines[0].Box.x,
                h = block.Lines[0].Box.h
            };
        }

        private static float CalculateAverageFontSize(TextBlockModel block)
        {
            return block.Lines.Average(l => l.AverageFontSize);
        }
    }
}