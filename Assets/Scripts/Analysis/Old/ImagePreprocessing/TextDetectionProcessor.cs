using System;
using Cysharp.Threading.Tasks;
using OpenCvSharp;
using UnityEngine;

namespace Analysis
{
    public class TextDetectionProcessor : MonoBehaviour
    {
        private TesseractDriver _tesseractDriver;
        private OcrWordsToTextStructureAdapter _ocrWordsToTextStructureAdapter;

        private void Awake()
        {
            _tesseractDriver = new TesseractDriver();
            _ocrWordsToTextStructureAdapter = new OcrWordsToTextStructureAdapter();
        }

        public async UniTask<OcrResult> Process(Texture2D texture)
        {
            var image = AnalysisUtils.TextureToMat(texture);

            var gray = new Mat();
            Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);
            var grayToTexture = AnalysisUtils.MatToTexture(gray);

            await _tesseractDriver.TryToSetup();
            var recognizedText = _tesseractDriver.Recognize(grayToTexture);
            return recognizedText;
        }

        public TextDocument GetTextDocument(OcrResult recognizedText)
        {
            var detectedWords = recognizedText.Text;
            var detectedBoxes = recognizedText.Boxes;

            if (detectedBoxes == null || detectedWords == null)
                throw new Exception("OCR not initialized");

            return _ocrWordsToTextStructureAdapter.Convert(detectedBoxes, detectedWords);
        }
    }
}