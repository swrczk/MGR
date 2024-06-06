using System;
using System.Collections.Generic;
using OpenCvSharp;

namespace Analysis
{
    public class WordPropertiesProcessor
    {
        public void ProcessWordProperties(List<WordModel> words, Mat mat)
        {
            var imageArea = mat.Width * mat.Height;

            foreach (var word in words)
            {
                word.ImagePercentage  = CalculateWordPercentage(word, imageArea);
            }
        }

        private static float CalculateWordPercentage(WordModel word, int imageArea)
        {
            var wordArea = word.Box.w * word.Box.h;
            var percentage = (float)wordArea / imageArea * 100;
            return percentage;
        }
    }
}