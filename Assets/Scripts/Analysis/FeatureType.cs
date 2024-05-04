namespace Analysis
{
    public enum FeatureType
    {
        HighContrastColors = 1,
        BrightColors = 2,
        LowTextBackgroundContrast = 3,
        ImageAsTextBackground = 4,
        NonSansSerifFont = 5,
        FontSmallerThan14pt = 6,
        SentenceLongerThan15Words = 7,
        InsufficientSpacingBetweenTextBlocks = 8,
        RightAlignedText = 9,
    }
}