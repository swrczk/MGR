using System.Collections.Generic;
using Analysis;
using UnityEngine;
using Utils;

namespace Manager
{
    public class ImageProcessingManager : BaseManager
    {
        [SerializeField]
        private ColorDetectorProcessor colorDetectorProcessor;

        [SerializeField]
        private int clusterCount = 5;

        private ScreenForAnalysisSo _screenForAnalysisData;
        public void SetupInput(ScreenForAnalysisSo screenForAnalysisData)
        {
            _screenForAnalysisData = screenForAnalysisData;
        }
        
        public List<Color> GetDominantColors()
        {
            return colorDetectorProcessor.DetectDominantColors(_screenForAnalysisData.texture, clusterCount);
        }
        
        public Texture2D GetSegmentedTexture(int dominantColorsCount)
        {
            return colorDetectorProcessor.SegmentImageUsingKMeans(_screenForAnalysisData.texture, dominantColorsCount);
        }
        
        public Texture2D DrawContours(Texture2D texture)
        {
            return colorDetectorProcessor.DrawContours(texture);
        }
    }
}