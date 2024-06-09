using System.Collections.Generic;
using UI;
using UnityEngine;

namespace Analysis
{
    [CreateAssetMenu(fileName = "AnalysisParameters", menuName = "AnalysisParametersSO")]
    public class AnalysisParametersSO : ScriptableObject
    { 
        public List<AnalysisParameter> parameters;

        public float GetWeight(FeatureType feature)
        {
            return parameters.Find(p => p.feature == feature).weight;
        }
    }
}