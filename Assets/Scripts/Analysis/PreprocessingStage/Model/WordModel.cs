using UnityEngine;

namespace Analysis
{
    public class WordModel
    {
        public Box Box { get; set; }
        public string Text { get; set; }
        public int FontSize { get; set; }
        public Color TextColor { get; set; }

        public Color TextBackgroundColor { get; set; }
        public bool IsUniformBackground { get; set; }

        public void SaveTextColor(Color color)
        {
            TextColor = color;
        }

        public void SaveBackgroundColor(Color color, bool isUniform)
        {
            TextBackgroundColor = color;
            IsUniformBackground = isUniform;
        }
    }
}