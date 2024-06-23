using System.Collections.Generic;
using UnityEngine;

namespace Analysis.PropertiesProcessingStage.Processor
{
    public class DominantColorsPropertiesProcessor
    {
        private ColorPropertiesProcessor _colorPropertiesProcessor;

        public DominantColorsPropertiesProcessor(ColorPropertiesProcessor colorPropertiesProcessor)
        {
            _colorPropertiesProcessor = colorPropertiesProcessor;
        }

        public Dictionary<(int, int), float> Process(List<Color> dominantColors)
        {
            var result = new Dictionary<(int, int), float>();
            for (var i = 0; i < dominantColors.Count-1; i++)
            {
                for (var j = i + 1; j < dominantColors.Count; j++)
                {
                    var contrast = _colorPropertiesProcessor.CalculateContrastRatio(dominantColors[i],
                        dominantColors[j]);
                    result.Add((i, j), contrast);
                }
            }

            return result;
        }
    }
}