namespace Analysis.PropertiesProcessingStage.Processor
{
    public class WordPropertiesProcessor
    {
        public void ProcessWordProperties(WordModel word, int imageArea)
        {
            word.ImagePercentage = CalculateWordPercentage(word, imageArea);
        }

        private static float CalculateWordPercentage(WordModel word, int imageArea)
        {
            var wordArea = word.Box.w * word.Box.h;
            var percentage = (float)wordArea / imageArea * 100;
            return percentage;
        }
    }
}