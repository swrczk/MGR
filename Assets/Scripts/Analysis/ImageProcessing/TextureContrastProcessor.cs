using OpenCvSharp;
using UnityEngine;
using Rect = OpenCvSharp.Rect;

namespace Analysis
{
    public class TextureContrastProcessor : MonoBehaviour
    {
        public void CheckContrastInBlocks(Texture2D inputTexture)
        {
            Mat image = ImageProcessingUtils.TextureToMat(inputTexture);
            int blockWidth = 10;
            int blockHeight = 10;

            // Liczba bloków w pionie i poziomie
            int numBlocksX = image.Width / blockWidth;
            int numBlocksY = image.Height / blockHeight;

            for (int i = 0; i < numBlocksX; i++)
            {
                for (int j = 0; j < numBlocksY; j++)
                {
                    Rect blockRect = new Rect(i * blockWidth, j * blockHeight, blockWidth, blockHeight);
                    Mat block = new Mat(image, blockRect);

                    Vec3d averageColor = GetAverageColor(block);
                    Debug.Log($"Average color of block ({i}, {j}): {averageColor}");
                }
            }
        }

        private Vec3d GetAverageColor(Mat block)
        {
            Scalar averageScalar = Cv2.Mean(block);
            return new Vec3d(averageScalar.Val0, averageScalar.Val1, averageScalar.Val2);
        }

        private void CompareBlocks(Vec3d[,] blockColors, int numBlocksX, int numBlocksY)
        {
            float someThreshold = 14.5f;

            for (int i = 0; i < numBlocksX - 1; i++)
            {
                for (int j = 0; j < numBlocksY - 1; j++)
                {
                    Vec3d colorDifferenceX =
                        ImageProcessingUtils.SubtractColors(blockColors[i + 1, j], blockColors[i, j]);
                    Vec3d colorDifferenceY =
                        ImageProcessingUtils.SubtractColors(blockColors[i, j + 1], blockColors[i, j]);

                    double contrastX = ImageProcessingUtils.CalculateNorm(colorDifferenceX);
                    double contrastY = ImageProcessingUtils.CalculateNorm(colorDifferenceY);

                    if (contrastX > someThreshold || contrastY > someThreshold)
                    {
                        Debug.Log($"High contrast detected at block ({i}, {j})");
                    }
                }
            }
        }
    }
}