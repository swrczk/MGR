using Analysis.AccessibilityProcessingStage.Processor;
using Analysis.PropertiesProcessingStage.Model;
using Analysis.PropertiesProcessingStage.Processor;
using UnityEngine;

namespace Analysis.AccessibilityProcessingStage
{
    public class AccessibilityModel
    {
    }

    public class AccessibilityProcessingStage
    {
        private ColorPropertiesProcessor _colorPropertiesProcessor;
        private TextAccessibilityProcessor _textAccessibilityProcessor;

        public AccessibilityProcessingStage(ColorPropertiesProcessor colorPropertiesProcessor)
        {
            _colorPropertiesProcessor = colorPropertiesProcessor;
            _textAccessibilityProcessor = new TextAccessibilityProcessor(colorPropertiesProcessor);
        }


        public AccessibilityModel Run(PropertiesModel model, Texture2D originalTexture)
        {
            return new AccessibilityModel();
        }
    }
}