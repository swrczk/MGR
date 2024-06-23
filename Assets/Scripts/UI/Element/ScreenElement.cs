using System;
using Analysis;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ScreenElement : MonoBehaviour
    {
        public event Action<string, ScreenElement> OnDelete;
        public event Action<string, bool> OnToggle; 

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
            checkbox.OnToggle += OnToggleChanged;
        }

        private void OnToggleChanged(bool isEnabled)
        {
            _screenModelData.isSelected = isEnabled;
            OnToggle?.Invoke(_screenModelData.uploadTimeStamp, isEnabled); // Invoke the toggle event
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
            checkbox.OnToggle -= OnToggleChanged;
        }

        private void Delete()
        {
            OnDelete?.Invoke(_screenModelData.uploadTimeStamp, this);
        }
    }
}