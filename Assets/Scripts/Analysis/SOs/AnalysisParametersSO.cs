using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UI;

namespace Analysis
{
    [CreateAssetMenu(fileName = "AnalysisParameters", menuName = "AnalysisParametersSO")]
    public class AnalysisParametersSO : ScriptableObject
    {
        public List<AnalysisParameter> parameters = new List<AnalysisParameter>()
        {
            new AnalysisParameter(FeatureType.HighTextBackgroundContrast, 0.1f, true),
            new AnalysisParameter(FeatureType.LowTextBackgroundContrast, 0.1f, true),
            new AnalysisParameter(FeatureType.HighContrastColors, 0.1f, true),
            new AnalysisParameter(FeatureType.BrightColors, 0.1f, true),
            new AnalysisParameter(FeatureType.TooSmallFont, 0.1f, true),
            new AnalysisParameter(FeatureType.InsufficientSpacingBetweenTextBlocks, 0.1f, true),
            new AnalysisParameter(FeatureType.RightAlignedText, 0.1f, true),
            new AnalysisParameter(FeatureType.TooSmallInterlineInTextBlock, 0.1f, true),
            new AnalysisParameter(FeatureType.SegmentsWithHighContrast, 0.1f, true),
            new AnalysisParameter(FeatureType.SegmentsWithLowContrast, 0.1f, true),
            new AnalysisParameter(FeatureType.DominantColorsWithLowContrast, 0.1f, true),
            new AnalysisParameter(FeatureType.DominantColorsWithHighContrast, 0.1f, true),
            new AnalysisParameter(FeatureType.SkipTextAnalysis, 1f, false),
        };

        private static string SavePath => Path.Combine(Application.persistentDataPath, "analysis_parameters.json");

        public float GetWeight(FeatureType feature)
        {
            return GetParameter(feature)?.weight ?? 0.1f;
        }

        public AnalysisParameter GetParameter(FeatureType feature)
        {
            return parameters.Find(p => p.feature == feature);
        }

        public void Save()
        {
            string json = JsonUtility.ToJson(this, true);
            File.WriteAllText(SavePath, json);
        }

        public void Load()
        {
            if (File.Exists(SavePath))
            {
                string json = File.ReadAllText(SavePath);
                JsonUtility.FromJsonOverwrite(json, this);
            }
        }
    }
}
