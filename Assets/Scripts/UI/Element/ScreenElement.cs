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

        private ScreenModel _screenModelData;

        private void Start()
        {
            deleteButton.onClick.AddListener(Delete);
            checkbox.OnToggle += OnToggle;
        }

        private void OnToggle(bool isEnabled)
        {
            _screenModelData.isSelected = isEnabled;
        }

        public void Setup(ScreenModel file)
        {
            image.texture = file.texture;
            _screenModelData = file;
            checkbox.Setup(_screenModelData.isSelected);
        }

        private void OnDestroy()
        {
            deleteButton.onClick.RemoveListener(Delete);
            checkbox.OnToggle -= OnToggle;
        }

        private void Delete()
        {
            OnDelete?.Invoke(_screenModelData.fileName, this);
        }
    }
}