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

        public static bool IsSmaller(Vec3b x, Vec3b y)
        {
            return x.Item0 < y.Item0 && x.Item1 < y.Item1 && x.Item2 < y.Item2;
        }

        public static bool IsBigger(Vec3b x, Vec3b y)
        {
            return x.Item0 > y.Item0 && x.Item1 > y.Item1 && x.Item2 > y.Item2;
        }

        public static Mat MatToGreyScale(Mat mat, bool keepColorChannels = false)
        {
            var gray = new Mat();
            Cv2.CvtColor(mat, gray, ColorConversionCodes.RGB2GRAY);
            if (keepColorChannels)
                Cv2.CvtColor(gray, gray, ColorConversionCodes.GRAY2BGR);
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
    }
}