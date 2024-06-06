using System;
using System.Collections.Generic;
using Analysis.PropertiesProcessingStage.Model;
using OpenCvSharp;

namespace Analysis.PropertiesProcessingStage.Processor
{
    public class TextDocumentPropertiesProcessor
    {
        public void ProcessTextDocumentProperties(TextDocumentModel document, Mat mat)
        {
            document.BlockDistances = CalculateBlockDistances(document);
            document.Margins = CalculateMargins(document, mat);
        }

        private Dictionary<(int, int), BlockDistanceInfoModel> CalculateBlockDistances(TextDocumentModel document)
        {
            var blockDistances = new Dictionary<(int, int), BlockDistanceInfoModel>();
            for (var i = 0; i < document.TextBlocks.Count - 1; i++)
            {
                var block1 = document.TextBlocks[i];
                var block2 = document.TextBlocks[i + 1];

                var distance = CalculateDistanceBetweenBlocks(block1, block2);

                var distanceInfo = new BlockDistanceInfoModel
                {
                    Block1Index = i,
                    Block2Index = i + 1,
                    Distance = distance
                };

                blockDistances[(i, i + 1)] = distanceInfo;
            }

            return blockDistances;
        }

        private float CalculateDistanceBetweenBlocks(TextBlockModel block1, TextBlockModel block2)
        {
            float bottomOfBlock1 = block1.Box.y + block1.Box.h;
            float topOfBlock2 = block2.Box.y;

            return topOfBlock2 - bottomOfBlock1;
        }

        private Margins CalculateMargins(TextDocumentModel document, Mat mat)
        {
            var imageWidth = mat.Width;
            var imageHeight = mat.Height;

            if (document.TextBlocks.Count == 0)
            {
                return new Margins
                {
                    Top = imageHeight,
                    Bottom = imageHeight,
                    Left = imageWidth,
                    Right = imageWidth
                };
            }

            float topMargin = document.TextBlocks[0].Box.y;
            float bottomMargin = imageHeight - (document.TextBlocks[^1].Box.y + document.TextBlocks[^1].Box.h);
            var leftMargin = float.MaxValue;
            var rightMargin = float.MaxValue;

            foreach (var block in document.TextBlocks)
            {
                if (block.Box.x < leftMargin)
                {
                    leftMargin = block.Box.x;
                }

                float blockRightMargin = imageWidth - (block.Box.x + block.Box.w);
                if (blockRightMargin < rightMargin)
                {
                    rightMargin = blockRightMargin;
                }
            }

            return new Margins
            {
                Top = topMargin,
                Bottom = bottomMargin,
                Left = leftMargin,
                Right = rightMargin
            };
        }
    }
}