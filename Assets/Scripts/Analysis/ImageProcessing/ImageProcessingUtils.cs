using System;
using OpenCvSharp;
using UnityEngine;

namespace Analysis
{
    public static class ImageProcessingUtils
    {
        public static Texture2D MatToTexture(Mat mat)
        {
            if (mat.Type() != MatType.CV_8UC4)
            {
                var tempMat = new Mat();
                switch (mat.Channels())
                {
                    case 1:
                        Cv2.CvtColor(mat, tempMat, ColorConversionCodes.GRAY2RGBA);
                        break;
                    case 3:
                        Cv2.CvtColor(mat, tempMat, ColorConversionCodes.BGR2RGBA);
                        break;
                    default:
                        return null;
                }

                mat = tempMat;
            }

            var texture = new Texture2D(mat.Width, mat.Height, TextureFormat.RGBA32, false);
            Cv2.Flip(mat, mat, FlipMode.X);
            texture.LoadRawTextureData(mat.Data, (int)(mat.Step() * mat.Rows));
            texture.Apply();
            return texture;
        }

        public static Mat TextureToMat(Texture2D texture)
        {
            return OpenCvSharp.Unity.TextureToMat(texture);
        }

        public static Vec3d SubtractColors(Vec3d color1, Vec3d color2)
        {
            return new Vec3d(color1.Item0 - color2.Item0, color1.Item1 - color2.Item1, color1.Item2 - color2.Item2);
        }

        public static double CalculateNorm(Vec3d vec)
        {
            return Math.Sqrt(vec.Item0 * vec.Item0 + vec.Item1 * vec.Item1 + vec.Item2 * vec.Item2);
        }
    }
}