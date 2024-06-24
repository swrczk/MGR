using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UI;

namespace Analysis
{
    [CreateAssetMenu(fileName = "AnalysisParameters", menuName = "AnalysisParametersSO")]
    public class AnalysisParametersSO : ScriptableObject
    {
        public List<AnalysisParameterModel> parameters = new()
        {
            new AnalysisParameterModel(FeatureType.DominantColorsWithLowContrast, 0.1f, true),
            new AnalysisParameterModel(FeatureType.DominantColorsWithHighContrast, 0.1f, true),
            new AnalysisParameterModel(FeatureType.SegmentsWithLowContrast, 0.1f, true),
            new AnalysisParameterModel(FeatureType.SegmentsWithHighContrast, 0.1f, true),
            new AnalysisParameterModel(FeatureType.LowTextBackgroundContrast, 0.1f, true),
            new AnalysisParameterModel(FeatureType.HighTextBackgroundContrast, 0.1f, true),
            new AnalysisParameterModel(FeatureType.TooSmallFont, 0.1f, true),
            new AnalysisParameterModel(FeatureType.TooSmallInterlineInTextBlock, 0.1f, true),
            new AnalysisParameterModel(FeatureType.RightAlignedText, 0.1f, true),
        };

        private static string SavePath => Path.Combine(Application.persistentDataPath, "analysis_parameters.json");

        private void OnEnable()
        {
            LoadFromJson();
        }

        public float GetWeight(FeatureType feature)
        {
            return GetParameter(feature)?.weight ?? 0.1f;
        }

        public AnalysisParameterModel GetParameter(FeatureType feature)
        {
            return parameters.Find(p => p.feature == feature);
        }

        public void SaveToJson()
        {
            var json = JsonUtility.ToJson(this, true);
            File.WriteAllText(SavePath, json);
        }

        public void LoadFromJson()
        {
            if (!File.Exists(SavePath))
            {
                SaveToJson();
            }

            var json = File.ReadAllText(SavePath);
            JsonUtility.FromJsonOverwrite(json, this);
        }
    }
}