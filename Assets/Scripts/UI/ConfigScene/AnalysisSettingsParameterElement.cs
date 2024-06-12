using TMPro;
using UnityEngine;

namespace UI
{
    public class AnalysisSettingsParameterElement : MonoBehaviour
    {
        [SerializeField]
        public CheckboxElement isEnabled;

        [SerializeField]
        private TMP_InputField weightInputField;

        public void Setup(AnalysisParameterModel arg)
        {
            isEnabled.Setup(arg.isEnabled);
            weightInputField.text = arg.weight.ToString();
        }

        public bool IsEnabled => true; //TODO: Implement this

        public float Weight
        {
            get
            {
                if (float.TryParse(weightInputField.text, out float result))
                {
                    return result;
                }

                return 0.1f; // Default weight value if parsing fails
            }
        }
    }
}