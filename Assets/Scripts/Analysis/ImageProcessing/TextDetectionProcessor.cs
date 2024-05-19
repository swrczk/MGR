using Cysharp.Threading.Tasks;
using OpenCvSharp;
using UnityEngine;
using Rect = OpenCvSharp.Rect;

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

        public async UniTask CheckSerifs(Texture2D texture)
        {
            var image = ImageProcessingUtils.TextureToMat(texture);

            var gray = new Mat();
            Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);
            var grayToTexture = ImageProcessingUtils.MatToTexture(gray);
            await _tesseractDriver.TryToSetup();
            _tesseractDriver.Recognize(grayToTexture);

            Cv2.Flip(gray, gray, FlipMode.X);
            var boxes = _tesseractDriver.GetTextBoxes();
            var words = _tesseractDriver.GetWords();
            var trues = 0;
            var alls = 0;
            for (var index = 0; index < boxes.Length; index++)
            {
                if (words[index].Length < 3) continue;
                alls++;
                var result = IsSerifFont(texture, boxes[index]);
                if (result)
                    trues++;
                Debug.LogError($" {words[index]} -> {result}");
            }
            
            Debug.LogError($"Serifs: {trues}/{alls} = {trues / (float)alls}");
        }
        private bool IsSerifFont(Texture2D image, Box box)
        {
            Mat imgMat = ImageProcessingUtils.TextureToMat(image);
            Rect roi = new Rect(box.x, box.y, box.w, box.h);
            Mat textRegion = new Mat(imgMat, roi);

            // Konwersja do skali szarości i detekcja krawędzi
            Mat gray = new Mat();
            Cv2.CvtColor(textRegion, gray, ColorConversionCodes.BGR2GRAY);
            Mat edges = new Mat();
            Cv2.Canny(gray, edges, 100, 200);

            // Analiza konturów
            Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(edges, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            // Przykładowa funkcja analizująca kontury pod kątem cech szeryfowych
            return HasSerifs(contours);
        }


        private bool HasSerifs(Point[][] contours)
        {
            foreach (var contour in contours)
            {
                Rect boundingBox = Cv2.BoundingRect(contour);
                double area = Cv2.ContourArea(contour);
                double aspectRatio = (double)boundingBox.Width / boundingBox.Height;
                double fillRatio = area / (boundingBox.Width * boundingBox.Height);
                if (fillRatio > 0.5 && aspectRatio < 2)
                {
                    return true; // Kontur prawdopodobnie zawiera szeryfy
                }
            }

            return false;
        }
    }
}