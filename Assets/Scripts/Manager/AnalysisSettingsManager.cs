using System.Collections.Generic;
using Analysis;
using UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace Manager
{
    public class AnalysisSettingsManager : BaseManager
    {
        [SerializeField]
        private List<SingleFeatureSettingDto> parameters;

        [FormerlySerializedAs("analysisParametersSo")]
        [SerializeField]
        private AnalysisParametersSO analysisParameters;

        private void Awake()
        {
            analysisParameters.Load();
            Apply();
        }

        private void OnApplicationQuit()
        {
            analysisParameters.Save();
        }


        private void Apply()
        {
            foreach (var param in analysisParameters.parameters)
            {
                var setting = parameters.Find(s => s.FeatureType == param.feature);
                if (setting != null)
                {
                    setting.Settings.Setup(param);
                }
            }
        }
    }
}