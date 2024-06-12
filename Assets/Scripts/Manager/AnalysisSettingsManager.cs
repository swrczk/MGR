using System.Collections.Generic;
using Analysis;
using UI;
using UnityEngine;

namespace Manager
{
    public class AnalysisSettingsManager : BaseManager
    {
        
        [SerializeField]
        private AnalysisParametersSO analysisParameters;

        private void Awake()
        {
            analysisParameters.Load();
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
            analysisParameters.Save();
        }

        private void OnApplicationQuit()
        {
            analysisParameters.Save();
        }
    }
}