using Analysis;
using UnityEngine;
using UnityEngine.UI;

public class ScreenElement : MonoBehaviour
{
    [SerializeField]
    private RawImage image;

    [SerializeField]
    private Button deleteButton;
    
    private ScreenDataSo screenData;

    private void Start()
    {
        deleteButton.onClick.AddListener(Delete);
    }

    public void Setup(ScreenDataSo file)
    {
        image.texture = file.texture;
        screenData = file;
    }

    private void OnDestroy()
    {
        deleteButton.onClick.RemoveListener(Delete);
    }
    
    private void Delete()
    {
        screenData.Delete();
        Destroy(gameObject);
    }
}