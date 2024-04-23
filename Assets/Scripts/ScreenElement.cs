using System;
using UnityEngine;
using UnityEngine.UI;

public class ScreenElement : MonoBehaviour
{
    [SerializeField]
    private RawImage image;

    [SerializeField]
    private Button deleteButton;

    // private void Start()
    // {
    //     deleteButton.onClick.AddListener(Delete);
    // }

    public void Setup(ScreenDataSo file)
    {
        image.texture = file.myTexture;
    }

    // private void OnDestroy()
    // {
    //     deleteButton.onClick.RemoveListener(Delete);
    // }
    //
    // private void Delete()
    // {
    // }
}