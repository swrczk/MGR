using System.Collections.Generic;
using System.Linq;
using Analysis.PreprocessingStage.Model.TextModels;

namespace Analysis.PropertiesProcessingStage.Processor
{
    public class TextBlockPropertiesProcessor
    {
        public void ProcessTextBlockProperties(TextBlockModel block, int imageArea)
        {
            block.Box = CalculateBoxSize(block);
            block.AverageFontSize = CalculateAverageFontSize(block);
            block.AverageInterline = CalculateAverageLineSpacing(block.Lines);
            block.IsLeftAligned = CheckLeftAlignment(block.Lines);
            block.ImageAreaPercentage = CalculatePercentageArea(block, imageArea);
        }

        private float CalculatePercentageArea(TextBlockModel block, int imageArea)
        {
            var blockArea = block.Box.w * block.Box.h;
            return blockArea / (float)imageArea * 100f;
        }

        private bool CheckLeftAlignment(List<LineModel> lines)
        {
            if (lines.Count < 1)
            {
                return false;
            }

            var firstLineX = lines[0].Box.x;
            return lines.All(line => line.Box.x == firstLineX);
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

        private float CalculateAverageFontSize(TextBlockModel block)
        {
            return block.Lines.Average(l => l.AverageFontSize);
        }
    }
}