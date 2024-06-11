using System;
using System.Collections.Generic;
using OpenCvSharp;
using UnityEngine;

namespace Analysis
{
    public static class ExtenstionAnalysis
    {
        public static Texture2D ToTexture(this Mat mat)
        {
            return AnalysisUtils.MatToTexture(mat);
        }

        public static Mat ToMat(this Texture2D texture)
        {
            return AnalysisUtils.TextureToMat(texture);
        }
        
        public static Vec3b ToVec3B(this Color color)
        {
            return AnalysisUtils.ColorToVec3B(color);
        }
    }

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

        public static Mat MatToGrayScale(Mat mat)
        {
            var gray = new Mat();
            if (mat.Channels() == 1) //already grayscale
            {
                gray = mat.Clone();
            }

            if (mat.Channels() == 3) //RGB
            {
                Cv2.CvtColor(mat, gray, ColorConversionCodes.RGB2GRAY);
            }

            if (mat.Channels() == 4) //RGBA
            {
                Cv2.CvtColor(mat, gray, ColorConversionCodes.RGBA2GRAY);
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
            return new Color(color.Item2 / 255f, color.Item1 / 255f, color.Item0 / 255f);
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
            return new Scalar(color.b * 255, color.g * 255, color.r * 255);
        }

        public static Vec3b ColorToVec3B(Color color)
        {
            return new Vec3b((byte)color.b, (byte)color.g, (byte)color.r);
        }
    }
}