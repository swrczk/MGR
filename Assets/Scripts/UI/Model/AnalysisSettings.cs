using System;
using System.Collections.Generic;
using Analysis;
using Analysis.DTO;
using UnityEngine;
using UnityEngine.Serialization;

public class AnalysisSettings : MonoBehaviour
{
    [Serializable]
    class Setting
    {
        [SerializeField]
        private FeatureType featureType;

        [SerializeField]
        private AnalysisSettingsParameter settings;
        
        public FeatureType FeatureType => featureType;
        public AnalysisSettingsParameter Settings => settings;
    }

    [SerializeField]
    private List<Setting> parameters;
    
    [FormerlySerializedAs("analysisParametersSO")]
    [SerializeField]
    private AnalysisParametersSO analysisParametersSo;

    private void Awake()
    {
        foreach (var param in analysisParametersSo.parameters)
        {
            var setting = parameters.Find(s => s.FeatureType == param.feature);
            setting.Settings.Setup(param);
        }
    }
}