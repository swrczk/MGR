using Cysharp.Threading.Tasks;
using OpenCvSharp;
using UnityEngine;
using Rect = OpenCvSharp.Rect;

namespace Analysis
{
    public class TextDetectorProcessor2 : MonoBehaviour
    {
         
        private TesseractDriver _tesseractDriver;
        
        [SerializeField]
        private TextAsset model;
        private void Awake()
        { 
            _tesseractDriver = new TesseractDriver(); 
        }
        public async UniTask<Texture2D> Process(Texture2D texture)
        {
            var image =  TextureToMat(texture);
            
            var gray = new Mat();
            Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);
            
            
            var grayToTexture =  MatToTexture(gray); 
            
            await _tesseractDriver.TryToSetup();
            var outputTexture = OnSetupCompleteRecognize(grayToTexture);
            return outputTexture;
        }

        private Texture2D OnSetupCompleteRecognize(Texture2D outputTexture)
        {
            var recognizedText = _tesseractDriver.Recognize(outputTexture);
            var x = _tesseractDriver.GetHighlightedTexture();
            Debug.LogError(recognizedText);
            return x;
        }


        private Mat TextureToMat(Texture2D texture)
        {
            var pixels = texture.GetPixels32();
            var mat = new Mat(texture.height, texture.width, MatType.CV_8UC4, pixels);
            return mat;
        }

        private Texture2D MatToTexture(Mat mat)
        {
            // Sprawdzenie i przekonwertowanie mat do formatu CV_8UC4, jeśli to konieczne
            if (mat.Type() != MatType.CV_8UC4)
            {
                var tempMat = new Mat();
                if (mat.Channels() == 1)
                {
                    Cv2.CvtColor(mat, tempMat, ColorConversionCodes.GRAY2RGBA);
                }
                else if (mat.Channels() == 3)
                {
                    Cv2.CvtColor(mat, tempMat, ColorConversionCodes.BGR2RGBA);
                }
                else
                {
                    return null;
                    // throw new InvalidOperationException("Unsupported Mat format. Must have 1 or 3 channels.");
                }

                mat = tempMat;
            }

            // Tworzenie Texture2D
            var texture = new Texture2D(mat.Width, mat.Height, TextureFormat.RGBA32, false);

            // Odwracanie obrazu, ponieważ OpenCV używa innego układu koordynatów
            // Cv2.Flip(mat, mat, FlipMode.X);

            // Wczytywanie danych obrazu do tekstury
            texture.LoadRawTextureData(mat.Data, (int)(mat.Step() * mat.Rows));

            // Aplikacja zmian
            texture.Apply();

            return texture;
        }
 

        private bool HasSerifs(Mat edges, Rect rect)
        {
            // Ekstrakcja regionu konturu
            var region = edges.SubMat(rect);

            // Analiza pikseli wzdłuż górnych i dolnych krawędzi regionu
            for (var x = 0; x < region.Width; x++)
            {
                // Sprawdzanie obecności serifików na górnej i dolnej krawędzi
                if (region.At<byte>(0, x) > 0 && region.At<byte>(region.Height - 1, x) > 0)
                    return true; // Serify zazwyczaj widoczne na krawędziach
            }

            return false;
        } 
    }
}