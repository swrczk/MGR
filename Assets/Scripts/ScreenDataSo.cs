using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Screen", menuName = "ScreenSO")]
public class ScreenDataSo : ScriptableObject
{ 
    [FormerlySerializedAs("FinalPath")]
    public string finalPath;
    public Texture myTexture;
}
