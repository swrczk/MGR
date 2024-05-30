using Cysharp.Threading.Tasks;
using OpenCvSharp;
using UnityEngine;

namespace Analysis
{
    public class TextDetectionProcessor : MonoBehaviour
    {
        [SerializeField]
        private int lowerThreshold = 200; // Zmniejsz, aby uwzględnić cieńsze linie

        [SerializeField]
        private int upperThreshold = 400; // Zwiększ, aby unikać fałszywych pozytywów w pogrubionych literach


        private TesseractDriver _tesseractDriver;

        private void Awake()
        {
            _tesseractDriver = new TesseractDriver();
        }

        public async UniTask<Texture2D> Process(Texture2D texture)
        {
            var image = ImageProcessingUtils.TextureToMat(texture);

            var gray = new Mat();
            Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);
            var grayToTexture = ImageProcessingUtils.MatToTexture(gray);

            await _tesseractDriver.TryToSetup();
            var recognizedText = _tesseractDriver.Recognize(grayToTexture);
            Debug.LogError(recognizedText);
            return _tesseractDriver.GetHighlightedTexture();
        }
    }
}