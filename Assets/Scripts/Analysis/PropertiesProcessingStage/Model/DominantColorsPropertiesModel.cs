using System.Collections.Generic;
using UnityEngine;

namespace Analysis.PropertiesProcessingStage.Model
{
    public class DominantColorsPropertiesModel
    {
        public List<Color> DominantColors { get; set; }
        public Dictionary<(int, int), float> ColorContrast { get; set; }
    }
}