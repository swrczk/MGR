using System.Collections.Generic;
using UnityEngine;

namespace Analysis.DTO
{
    [CreateAssetMenu(fileName = "AnalysisParameters", menuName = "AnalysisParametersSO")]
    public class AnalysisParametersSO : ScriptableObject
    { 
        public List<AnalysisParameter> parameters;

    }
}