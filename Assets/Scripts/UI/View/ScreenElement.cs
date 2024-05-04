using System;
using Analysis;
using UnityEngine;
using UnityEngine.UI;

public class ScreenElement : MonoBehaviour
{
    public event Action<string> OnDelete;

    [SerializeField]
    private RawImage image;

    [SerializeField]
    private Button deleteButton;

    private ScreenDTO screenData;

    private void Start()
    {
        deleteButton.onClick.AddListener(Delete);
    }

    public void Setup(ScreenDTO file)
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
        OnDelete?.Invoke(screenData.name);
        Destroy(gameObject);
    }
}