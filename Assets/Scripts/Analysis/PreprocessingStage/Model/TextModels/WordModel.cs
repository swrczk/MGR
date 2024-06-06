using UnityEngine;

namespace Analysis
{
    public class WordModel
    {
        public Box Box { get; }
        public string Text { get; }
        public Color TextColor { get; set; }
        public Color TextBackgroundColor { get; set; }
        public bool IsUniformBackground { get; set; }
        public float ImageAreaPercentage { get; set; }
        public int FontSize => Box.h;

        public WordModel(Box box, string word)
        {
            Box = box;
            Text = word;
        }
    }
}