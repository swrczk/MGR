using TMPro;
using UnityEngine;

namespace UI
{
    public class AnalysisSettingsParameter : MonoBehaviour
    {
        [SerializeField]
        private CheckboxElement isEnabled;

        [SerializeField]
        private TMP_InputField weightInputField;

        public void Setup(AnalysisParameter arg)
        {
            isEnabled.Setup(arg.isEnabled);
            weightInputField.text = arg.weight.ToString();
        }
    }
}