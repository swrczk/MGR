using Analysis;
using UI.Common;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ImageProcessingView : BaseMainView
    {
        public ScreenForAnalysisSo ScreenForAnalysisData => screenForAnalysisData;

        [SerializeField]
        private ScreenForAnalysisSo screenForAnalysisData;

        [SerializeField]
        private RawImage originalImage;

        [SerializeField]
        private RawImage processedImage;


        public void Start()
        {
            originalImage.texture = screenForAnalysisData.texture;
        }

        public void ShowProcessedImage(Texture2D preview)
        {
            processedImage.texture = preview;
        }
    }
}