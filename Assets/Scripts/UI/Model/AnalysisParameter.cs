using Analysis;
using UnityEngine;

namespace UI
{
    [System.Serializable]
    public class AnalysisParameter
    {
        public FeatureType feature;

        [Range(0.01f, 1f)]
        public float weight;

        public bool isEnabled;

        public AnalysisParameter(FeatureType feature, float weight, bool isEnabled)
        {
            this.feature = feature;
            this.weight = weight;
            this.isEnabled = isEnabled;
        }
    }
}