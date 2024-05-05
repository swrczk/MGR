using System.Collections.Generic;
using UI;
using UnityEngine;

namespace Analysis
{
    [CreateAssetMenu(fileName = "AnalysisParameters", menuName = "AnalysisParametersSO")]
    public class AnalysisParametersSO : ScriptableObject
    { 
        public List<AnalysisParameter> parameters;

    }
}