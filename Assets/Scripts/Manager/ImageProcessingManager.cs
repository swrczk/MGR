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

        private ScreenSo _screenData;
        public void SetupInput(ScreenSo screenData)
        {
            _screenData = screenData;
            // _screenData.SaveColorSegmentation(segmentation);
            // _screenData.SaveDominantColors(dominantColors);
        }
        
        public List<Color> GetDominantColors()
        {
            return colorDetectorProcessor.DetectDominantColors(_screenData.texture, clusterCount);
        }
        
        public Texture2D GetSegmentedTexture(int dominantColorsCount)
        {
            return colorDetectorProcessor.SegmentImageUsingKMeans(_screenData.texture, dominantColorsCount);
        }
        
        public Texture2D DrawContours(Texture2D texture)
        {
            return colorDetectorProcessor.DrawContours(texture);
        }
    }
}