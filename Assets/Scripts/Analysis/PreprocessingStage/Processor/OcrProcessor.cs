using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Analysis
{
    public class OcrProcessor
    {
        private TesseractDriver _tesseractDriver;
        private OcrWordsToTextStructureAdapter _ocrWordsToTextStructureAdapter;

        public OcrProcessor()
        {
            _tesseractDriver = new TesseractDriver();
            _ocrWordsToTextStructureAdapter = new OcrWordsToTextStructureAdapter();
        }
        public async UniTask<TextDocumentModel> ProcessText(Texture2D texture)
        {
            var mat = AnalysisUtils.TextureToMat(texture);
            var grayMat = AnalysisUtils.MatToGrayScale(mat);
            var grayTexture = AnalysisUtils.MatToTexture(grayMat);
            var ocrResult = await RecognizeTextStructure(grayTexture) ;
            var documentStructure = GetTextDocument(ocrResult);
            return documentStructure;
        }

        public async UniTask<OcrResult> RecognizeTextStructure(Texture2D grayTexture)
        {
            await _tesseractDriver.TryToSetup();
            var recognizedText = _tesseractDriver.Recognize(grayTexture);
            return recognizedText;
        }

        public TextDocumentModel GetTextDocument(OcrResult recognizedText)
        {
            var detectedWords = recognizedText.Text;
            var detectedBoxes = recognizedText.Boxes;

            if (recognizedText.Text== null || recognizedText.Text == null)
                throw new Exception("OCR not initialized");

            return _ocrWordsToTextStructureAdapter.Convert(detectedBoxes, detectedWords);
        }
    }
}