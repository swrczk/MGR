using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class AnalysisSettingsParameter : MonoBehaviour
{
    [FormerlySerializedAs("isEnabledCheckBox")]
    [SerializeField]
    private CheckboxElement isEnabledCheckboxElement;

    [SerializeField]
    private TMP_InputField weightInputField;

    public void Setup(AnalysisParameter arg)
    {
        isEnabledCheckboxElement.Setup(arg.isEnabled);
        weightInputField.text = arg.weight.ToString();
    }
}