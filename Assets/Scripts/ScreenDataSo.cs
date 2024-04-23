using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Screen", menuName = "ScreenSO")]
public class ScreenDataSo : ScriptableObject
{
    public Texture texture;
    public string texturePath;
    public string thisObjectPath;
    
    public void Delete()
    {
        AssetDatabase.DeleteAsset(texturePath);
        AssetDatabase.DeleteAsset(thisObjectPath);
        AssetDatabase.SaveAssets();
    }
}
