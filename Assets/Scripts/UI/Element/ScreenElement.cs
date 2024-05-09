using System;
using Analysis;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ScreenElement : MonoBehaviour
    {
        public event Action<string, ScreenElement> OnDelete;

        [SerializeField]
        private RawImage image;

        [SerializeField]
        private Button deleteButton;

        [SerializeField]
        private CheckboxElement checkbox;

        private ScreenForAnalysisSo _screenForAnalysisData;

        private void Start()
        {
            deleteButton.onClick.AddListener(Delete);
            checkbox.OnToggle += OnToggle;
        }

        private void OnToggle(bool isEnabled)
        {
            _screenForAnalysisData.isSelected = isEnabled;
        }

        public void Setup(ScreenForAnalysisSo file)
        {
            image.texture = file.texture;
            _screenForAnalysisData = file;
            checkbox.Setup(_screenForAnalysisData.isSelected);
        }

        private void OnDestroy()
        {
            deleteButton.onClick.RemoveListener(Delete);
            checkbox.OnToggle -= OnToggle;
        }

        private void Delete()
        {
            OnDelete?.Invoke(_screenForAnalysisData.name, this);
        }
    }
}