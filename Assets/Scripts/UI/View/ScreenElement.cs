using System;
using Analysis;
using UnityEngine;
using UnityEngine.UI;

public class ScreenElement : MonoBehaviour
{
    public event Action<string, ScreenElement> OnDelete;

    [SerializeField]
    private RawImage image;

    [SerializeField]
    private Button deleteButton;

    private ScreenSo _screenData;

    private void Start()
    {
        deleteButton.onClick.AddListener(Delete);
    }

    public void Setup(ScreenSo file)
    {
        image.texture = file.texture;
        _screenData = file;
    }

    private void OnDestroy()
    {
        deleteButton.onClick.RemoveListener(Delete);
    }

    private void Delete()
    {
        OnDelete?.Invoke(_screenData.name, this);
    }
}