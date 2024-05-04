using System;
using UI.Common;
using UnityEngine;
using UnityEngine.UI;

public class StartView : BaseView
{
    public event Action UploadButtonClicked    ;
    public ScreenElement FilePrefab => filePrefab;
    public Transform ParentTransform => parentTransform;

    [SerializeField]
    private ScreenElement filePrefab;

    [SerializeField]
    private Transform parentTransform;
    
    [SerializeField]
    private Button uploadButton;

    protected override void RegisterEvents()
    {
        base.RegisterEvents();
        uploadButton.onClick.AddListener(OnUploadButtonClick);
    }
    
    protected override void UnregisterEvents()
    {
        base.UnregisterEvents();
        uploadButton.onClick.RemoveListener(OnUploadButtonClick);
    } 
    private void OnUploadButtonClick()
    {
        UploadButtonClicked?.Invoke();
    }
}