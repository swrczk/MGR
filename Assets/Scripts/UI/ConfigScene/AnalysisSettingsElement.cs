using System;
using System.Collections.Generic;
using Analysis;
using UnityEngine;
using UnityEngine.UI;

namespace UI.ConfigScene
{
    public class AnalysisSettingsElement : MonoBehaviour
    {
        public event Action SaveButtonClicked;

        public List<SingleFeatureSettingModel> FeatureSettings => _featureSettings;

        [SerializeField]
        private List<SingleFeatureSettingModel> _featureSettings;

        [SerializeField]
        private Button _saveButton;

        private void Start()
        {
            _saveButton.onClick.AddListener(() => SaveButtonClicked?.Invoke());
        }
        public void ApplyAnalysisParameters(AnalysisParametersSO analysisParameters)
        {
            foreach (var param in analysisParameters.parameters)
            {
                var setting = _featureSettings.Find(f => f.FeatureType == param.feature);
                if (setting != null)
                {
                    setting.Settings.Setup(param);
                }
            }
        }

        private void OnDestroy()
        {
            _saveButton.onClick.RemoveAllListeners();
        }
    }
}