using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UI;

namespace Analysis
{
    [CreateAssetMenu(fileName = "AnalysisParameters", menuName = "AnalysisParametersSO")]
    public class AnalysisParametersSO : ScriptableObject
    {
        public List<AnalysisParameterModel> parameters = new List<AnalysisParameterModel>()
        {
            // new AnalysisParameter(FeatureType.HighContrastColors, 0.1f, true),
            // new AnalysisParameter(FeatureType.BrightColors, 0.1f, true),
            // new AnalysisParameter(FeatureType.InsufficientSpacingBetweenTextBlocks, 0.1f, true),
            new AnalysisParameterModel(FeatureType.HighTextBackgroundContrast, 0.1f, true),
            new AnalysisParameterModel(FeatureType.LowTextBackgroundContrast, 0.1f, true),
            new AnalysisParameterModel(FeatureType.TooSmallFont, 0.1f, true),
            new AnalysisParameterModel(FeatureType.RightAlignedText, 0.1f, true),
            new AnalysisParameterModel(FeatureType.TooSmallInterlineInTextBlock, 0.1f, true),
            new AnalysisParameterModel(FeatureType.SegmentsWithHighContrast, 0.1f, true),
            new AnalysisParameterModel(FeatureType.SegmentsWithLowContrast, 0.1f, true),
            new AnalysisParameterModel(FeatureType.DominantColorsWithLowContrast, 0.1f, true),
            new AnalysisParameterModel(FeatureType.DominantColorsWithHighContrast, 0.1f, true),
            
            new AnalysisParameterModel(FeatureType.SkipTextAnalysis, 1f, false),
        };

        private static string SavePath => Path.Combine(Application.persistentDataPath, "analysis_parameters.json");

        public float GetWeight(FeatureType feature)
        {
            return GetParameter(feature)?.weight ?? 0.1f;
        }

        public AnalysisParameterModel GetParameter(FeatureType feature)
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
