using TMPro;
using UnityEngine;

public class AnalysisSettingsParameter : MonoBehaviour
{
    [SerializeField]
    private CheckBox isEnabledCheckBox;

    [SerializeField]
    private TMP_InputField weightInputField;

    public void Setup(AnalysisParameter arg)
    {
        isEnabledCheckBox.Setup(arg.isEnabled);
        weightInputField.text = arg.weight.ToString();
    }
}