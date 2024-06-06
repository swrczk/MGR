using UnityEngine;

namespace Analysis
{
    [CreateAssetMenu(fileName = "ImageProcessingParametersSo", menuName = "Analysis/ImageProcessingParametersSo")]
    public class ImageProcessingParametersSo : ScriptableObject
    {
        [Header("PreprocessingStage")]
        public int clusterCount = 5;
        public bool excludeWords = true;

        [Header("ColorProcessor")]
        public Color defaultContoursColor = Color.red;

        [Header("SegmentationProcessor")]
        public float targetColorAmount = 3;
        public float colorThreshold = 50;
        public float entropyThreshold = 0.3f;

        [Space(40)]
        [Header("PropertiesProcessingStage")]
        public float contrastLowerThreshold = 4.5f;

        public float contrastUpperThreshold = 15.0f;
        
        [Header("WordsColorProcessor")]
        public float entropyThresholdWords = 5.0f;
        public int edgesPerCharThreshold = 10;
    }
}