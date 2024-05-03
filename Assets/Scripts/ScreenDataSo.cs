using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Screen", menuName = "ScreenSO")]
public class ScreenDataSo : ScriptableObject
{
    public Texture2D texture;
    public string texturePath;
    public string thisObjectPath;
    public List<Color> dominantColors;
    public Texture2D colorSegmentation;

    public void Delete()
    {
        AssetDatabase.DeleteAsset(texturePath);
        AssetDatabase.DeleteAsset(thisObjectPath);
        AssetDatabase.SaveAssets();
    }

    public void SaveColorSegmentation(Texture2D texture2D)
    {
        colorSegmentation = texture2D;
        var path = texturePath.Split(".asset")[0];
        AssetDatabase.CreateAsset(texture2D, path + "_colorSegmentation" + ".asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        SaveTextureAsPNG(texture2D, path + "_colorSegmentation.png");
    }

    public void SaveDominantColors(List<Color> colors)
    {
        dominantColors = colors;
    }
    private void SaveTextureAsPNG(Texture2D texture2D, string filePath)
    { 
        byte[] pngBytes = texture2D.EncodeToPNG(); 
        File.WriteAllBytes(filePath, pngBytes); 
    }
}