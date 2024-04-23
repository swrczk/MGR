using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AnalysisParameters", menuName = "AnalysisParametersSO")]
public class AnalysisParametersSO : ScriptableObject
{ 
    public List<AnalysisParameter> parameters;

}