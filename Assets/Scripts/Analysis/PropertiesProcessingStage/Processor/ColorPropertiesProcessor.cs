using System;
using OpenCvSharp;
using UnityEngine;

namespace Analysis.PropertiesProcessingStage.Processor
{
    public class ColorPropertiesProcessor
    {
        // values from 0 to 1
        public float CalculateLuminance(Vec3b color)
        {
            // Normalizuj wartości RGB do zakresu 0-1
            var r = color.Item2 / 255.0f;
            var g = color.Item1 / 255.0f;
            var b = color.Item0 / 255.0f;

            // Skoryguj wartości RGB
            var rPrime = (float)((r <= 0.03928) ? r / 12.92 : Math.Pow((r + 0.055) / 1.055, 2.4));
            var gPrime = (float)((g <= 0.03928) ? g / 12.92 : Math.Pow((g + 0.055) / 1.055, 2.4));
            var bPrime = (float)((b <= 0.03928) ? b / 12.92 : Math.Pow((b + 0.055) / 1.055, 2.4));

            // Oblicz luminancję
            return 0.2126f * rPrime + 0.7152f * gPrime + 0.0722f * bPrime;
        }


        //do obliczania kontrastu tekstu z tłem
        //values from 1 to 21
        public float CalculateContrastRatio(Vec3b color1, Vec3b color2)
        {
            var luminance1 = CalculateLuminance(color1);
            var luminance2 = CalculateLuminance(color2);

            // Upewnij się, że luminance1 jest większe niż luminance2
            if (luminance1 < luminance2)
            {
                (luminance1, luminance2) = (luminance2, luminance1);
            }

            return (luminance1 + 0.05f) / (luminance2 + 0.05f);
        }

        public float CalculateContrastRatio(Color color1, Color color2)
        {
            return CalculateContrastRatio(color1.ToVec3B(), color2.ToVec3B());
        }
    }
}