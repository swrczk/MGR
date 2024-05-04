using UnityEngine;
using UnityEngine.UI;

public class CheckboxElement : MonoBehaviour
{
    [SerializeField]
    private Image checkImage;

    [SerializeField]
    private Button button;

    private bool IsEnabled { get; set; }


    protected void Awake()
    {
        button.onClick.AddListener(Toggle);
    }

    protected void OnDestroy()
    {
        button.onClick.RemoveListener(Toggle);
    }

    public void Setup(bool isEnabled)
    {
        IsEnabled = isEnabled;
        checkImage.enabled = IsEnabled;
    }

    private void Toggle()
    {
        IsEnabled = !IsEnabled;
        checkImage.enabled = IsEnabled;
    }
}