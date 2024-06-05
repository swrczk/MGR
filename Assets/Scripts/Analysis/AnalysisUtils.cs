using System;
using System.Collections.Generic;
using OpenCvSharp;
using UnityEngine;

namespace Analysis
{
    public class Vec3BEqualityComparer : IEqualityComparer<Vec3b>
    {
        public bool Equals(Vec3b x, Vec3b y)
        {
            return x.Item0 == y.Item0 && x.Item1 == y.Item1 && x.Item2 == y.Item2;
        }

        public int GetHashCode(Vec3b obj)
        {
            return obj.GetHashCode();
        }
    }

    public static class AnalysisUtils
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

        public static bool HasColorLowerValue(Color x, Color y)
        {
            return x.r < y.r && x.g < y.g && x.b < y.b;
        }

        public static bool HasColorBiggerValue(Color x, Color y)
        {
            return x.r > y.r && x.g > y.g && x.b > y.b;
        }


        public static Mat MatToGrayScale(Mat mat, bool keepColorChannels = false)
        {
            Mat gray = new Mat();

            // Handle different cases based on the number of channels
            if (mat.Channels() == 1)
            {
                // If the image is already grayscale, just copy it
                gray = mat.Clone();
            }
            else if (mat.Channels() == 3)
            {
                // If the image is RGB, convert it to grayscale
                Cv2.CvtColor(mat, gray, ColorConversionCodes.RGB2GRAY);
            }
            else if (mat.Channels() == 4)
            {
                // If the image is RGBA, convert it to grayscale
                Cv2.CvtColor(mat, gray, ColorConversionCodes.RGBA2GRAY);
            }
            else
            {
                // Handle unexpected number of channels
                throw new Exception($"Unsupported number of channels: {mat.Channels()}");
            }

            if (keepColorChannels)
            {
                // Convert grayscale image back to BGR (3 channels)
                Cv2.CvtColor(gray, gray, ColorConversionCodes.GRAY2BGR);
            }

            return gray;
        }

        public static Mat AddColorChannelsToGrayScale(Mat mat)
        {
            var gray = new Mat();
            Cv2.CvtColor(mat, gray, ColorConversionCodes.GRAY2BGR);
            return gray;
        }

        public static Color Vec3ToColor(Vec3b color)
        {
            return new Color(color.Item0, color.Item1, color.Item2);
        }

        public static Color Vec3ToColor(Vec3d color)
        {
            return new Color((int)color.Item0, (int)color.Item1, (int)color.Item2);
        }

        // ReSharper disable once InconsistentNaming
        public static Vec3b Vec3dToVec3b(Vec3d color)
        {
            return new Vec3b((byte)color.Item0, (byte)color.Item1, (byte)color.Item2);
        }


        //Result Range <0;8>
        public static float GetHistogramEntropy(Mat mat, bool convertToGreyScale = false)
        {
            var gray = convertToGreyScale ? MatToGrayScale(mat) : mat;

            // Analiza histogramu kolorów
            var hist = new Mat();
            int[] histSize = { 256 };
            Rangef[] ranges = { new Rangef(0, 256) };
            Cv2.CalcHist(new Mat[] { gray }, new int[] { 0 }, null, hist, 1, histSize, ranges);

            // Normalizacja histogramu
            Cv2.Normalize(hist, hist, 1.0, 0.0, NormTypes.L1);

            // Obliczenie entropii histogramu
            var entropy = 0.0f;
            for (var i = 0; i < hist.Rows; i++)
            {
                var binVal = hist.At<float>(i);
                if (binVal > 0)
                {
                    entropy -= binVal * (float)Math.Log(binVal, 2);
                }
            }

            return entropy;
        }

        public static Scalar ColorToScalar(Color color)
        {
            return new Scalar(color.r, color.g, color.b);
        }
    }
}