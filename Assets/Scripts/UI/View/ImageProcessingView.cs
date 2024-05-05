using Analysis;
using UI.Common;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ImageProcessingView : BaseMainView
    {
        [SerializeField]
        private ScreenSo screenData;

        [SerializeField]
        private RawImage originalImage;

        [SerializeField]
        private RawImage processedImage;

        public override void Setup()
        {
            base.Setup();
            originalImage.texture = screenData.texture;
        }
    }
}