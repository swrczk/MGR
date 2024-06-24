using System.Collections.Generic;
using Analysis;
using UI;
using UnityEngine;

namespace Manager
{
    public class AnalysisSettingsManager : BaseManager
    {
        
        public AnalysisParametersSO AnalysisParameters => analysisParameters;
        [SerializeField]
        private AnalysisParametersSO analysisParameters;

        private void Awake()
        {
            analysisParameters.LoadFromJson();
        }
        public void UpdateAndSaveParameters(List<SingleFeatureSettingModel> featureSettings)
        {
            foreach (var setting in featureSettings)
            {
                var parameter = analysisParameters.GetParameter(setting.FeatureType);
                if (parameter != null)
                {
                    parameter.weight = setting.Settings.Weight;
                    parameter.isEnabled = setting.Settings.isEnabled;
                }
                else
                {
                    analysisParameters.parameters.Add(new AnalysisParameterModel(setting.FeatureType, setting.Settings.Weight, setting.Settings.isEnabled));
                }
            }
            analysisParameters.SaveToJson();
        }

        private void OnApplicationQuit()
        {
            analysisParameters.SaveToJson();
        }
    }
}