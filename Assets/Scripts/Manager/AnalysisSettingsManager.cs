using System.Collections.Generic;
using Analysis;
using UI;
using UnityEngine;
using Utils;

namespace Manager
{
    public class AnalysisSettingsManager : BaseManager
    {

        [SerializeField]
        private List<SingleFeatureSettingDto> parameters;
    
        [SerializeField]
        private AnalysisParametersSO analysisParametersSo;

        private void Apply()
        {
            foreach (var param in analysisParametersSo.parameters)
            {
                var setting = parameters.Find(s => s.FeatureType == param.feature);
                setting.Settings.Setup(param);
            }
        }
    }
}