using System;
using UnityEngine;

namespace UI
{
    [Serializable]
    public class SingleFeatureSettingDto
    {
        [SerializeField]
        private FeatureType featureType;

        [SerializeField]
        private AnalysisSettingsParameter settings;

        public FeatureType FeatureType => featureType;
        public AnalysisSettingsParameter Settings => settings;
    }
}