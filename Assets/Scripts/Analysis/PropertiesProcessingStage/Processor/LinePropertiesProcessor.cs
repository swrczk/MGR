using System.Linq;

namespace Analysis.PropertiesProcessingStage.Processor
{
    public class LinePropertiesProcessor
    {
        public void ProcessLineProperties(LineModel line, int imageArea)
        {
            line.Box = CalculateLineBox(line);
            line.AverageFontSize = CalculateAverageFontSize(line);
            line.AverageSpacing = CalculateAverageSpacing(line);
            line.ImagePercentage = CalculateLinePercentage(line, imageArea);
        }

        private float CalculateLinePercentage(LineModel line, int imageArea)
        {
            var lineArea = line.Box.w * line.Box.h;
            return (float)lineArea / imageArea * 100;
        }

        private float CalculateAverageSpacing(LineModel line)
        {
            if (line.Words.Count < 2)
            {
                return 0;
            }

            float totalDistance = 0;
            for (var i = 0; i < line.Words.Count - 1; i++)
            {
                var distance = line.Words[i + 1].Box.x - (line.Words[i].Box.x + line.Words[i].Box.w);
                totalDistance += distance;
            }

            return totalDistance / (line.Words.Count - 1);
        }

        private Box CalculateLineBox(LineModel line)
        {
            return new Box
            {
                x = line.Words[0].Box.x,
                y = line.Words[0].Box.y,
                w = line.Words[^1].Box.x + line.Words[^1].Box.w - line.Words[0].Box.x,
                h = line.Words[0].Box.h
            };
        }

        private static float CalculateAverageFontSize(LineModel line)
        {
            return (float)line.Words.Average(w => w.FontSize);
        }
    }
}