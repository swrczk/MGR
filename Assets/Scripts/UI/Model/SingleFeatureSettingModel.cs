using System;
using UnityEngine;

namespace UI
{
    [Serializable]
    public class SingleFeatureSettingModel
    {
        [SerializeField]
        public FeatureType featureType;

        [SerializeField]
        public AnalysisSettingsParameterElement settings;

        public FeatureType FeatureType => featureType;
        public AnalysisSettingsParameterElement Settings => settings;
    }
}